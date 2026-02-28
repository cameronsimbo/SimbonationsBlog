using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Learn.Application.Common.Interfaces;
using Learn.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Learn.Infrastructure.Services;

public class ClaudeOptions
{
    public const string SectionName = "Claude";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "claude-haiku-4-5-20251001";
    public int TimeoutSeconds { get; set; } = 15;
}

public class ClaudeAIService : IAIEvaluationService
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;
    private readonly OllamaService _ollamaFallback;
    private readonly ILogger<ClaudeAIService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ClaudeAIService(
        HttpClient httpClient,
        IOptions<ClaudeOptions> options,
        OllamaService ollamaFallback,
        ILogger<ClaudeAIService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _ollamaFallback = ollamaFallback;
        _logger = logger;
    }

    public async Task<AIEvaluationResult> EvaluateAnswerAsync(EvaluationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            string prompt = BuildEvaluationPrompt(request);
            string response = await CallClaudeAsync(prompt, maxTokens: 512, cancellationToken);
            return ParseEvaluationResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Claude grading failed, falling back to Ollama");
            return await _ollamaFallback.EvaluateAnswerAsync(request, cancellationToken);
        }
    }

    public async Task<List<GeneratedExercise>> GenerateExercisesAsync(ExerciseGenerationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            string prompt = BuildGenerationPrompt(request);
            string response = await CallClaudeAsync(prompt, maxTokens: 2048, cancellationToken);
            return ParseGenerationResponse(response, request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Claude exercise generation failed, falling back to Ollama");
            return await _ollamaFallback.GenerateExercisesAsync(request, cancellationToken);
        }
    }

    private async Task<string> CallClaudeAsync(string prompt, int maxTokens, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = _options.Model,
            max_tokens = maxTokens,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        string json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages");
        request.Content = content;
        request.Headers.Add("x-api-key", _options.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(request, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        string responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        using JsonDocument doc = JsonDocument.Parse(responseJson);

        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }

    private static string BuildEvaluationPrompt(EvaluationRequest request)
    {
        return $$"""
            You are an AI tutor evaluating a student's answer. Be moderately strict: award partial credit for demonstrating understanding even if the answer is incomplete, but penalise factual inaccuracies and missing key concepts.

            EXERCISE DETAILS:
            - Type: {{request.ExerciseType}}
            - Subject: {{request.SubjectDomain}}
            - Difficulty: {{request.DifficultyLevel}}
            - Question: {{request.Prompt}}
            - Reference Answer: {{request.ReferenceAnswer}}

            STUDENT'S ANSWER:
            {{request.UserAnswer}}

            GRADING GUIDELINES:
            - Score 0-100 where 60+ is passing
            - 90-100: Excellent — comprehensive, accurate, well-articulated
            - 75-89: Good — mostly correct with minor gaps or clarity issues
            - 60-74: Adequate — demonstrates basic understanding but has notable gaps
            - 40-59: Below average — some relevant points but significant errors or omissions
            - 0-39: Poor — largely incorrect or irrelevant
            - An empty or nonsensical answer should score 0-10
            - Award partial credit when the student shows understanding of core concepts even if expression is imperfect
            - Penalise factual errors proportionally to their severity

            Respond with ONLY a valid JSON object in this exact format (no markdown, no extra text):
            {"score": <integer 0-100>, "isPassing": <true/false>, "feedback": "<2-3 sentences of constructive feedback>", "suggestedCorrection": "<brief correction or null if not needed>", "detailedBreakdown": "<brief breakdown of scoring: accuracy, depth, clarity>"}
            """;
    }

    private static string BuildGenerationPrompt(ExerciseGenerationRequest request)
    {
        string context = request.LessonContext ?? request.LessonName;
        string guidance = request.GenerationGuidance ?? "Focus on conceptual understanding and critical analysis.";
        string keyConcepts = request.KeyConcepts ?? "the core concepts of the lesson";
        string seedInfo = "";

        if (!string.IsNullOrWhiteSpace(request.SeedPrompt))
        {
            seedInfo = $"\nExample exercise for reference (generate DIFFERENT ones):\n- Prompt: {request.SeedPrompt}\n- Reference answer: {request.SeedReferenceAnswer}";
        }

        return $$"""
            You are an expert educational content creator. Generate {{request.Count}} exercises for a learning platform.

            CONTEXT:
            - Topic: {{request.TopicName}}
            - Unit: {{request.UnitName}}
            - Lesson: {{request.LessonName}}
            - Subject Domain: {{request.SubjectDomain}}
            - Difficulty: {{request.DifficultyLevel}}
            - Key Concepts: {{keyConcepts}}
            - Lesson Context: {{context}}
            - Guidance: {{guidance}}
            {{seedInfo}}

            EXERCISE TYPES TO USE (mix them):
            - Explanation: Ask the student to explain a concept
            - ReadingComprehension: Provide context and ask analysis questions
            - FreeTextResponse: Open-ended questions requiring thoughtful answers

            REQUIREMENTS:
            - Each exercise must have a clear prompt, optional context, a comprehensive reference answer, and a hint
            - Reference answers should be detailed enough for AI grading (3-5 sentences)
            - Questions should progress from foundational to more analytical
            - Difficulty should match: {{request.DifficultyLevel}}
            - Make questions specific to the lesson content, not generic

            Respond with ONLY a valid JSON array (no markdown, no extra text):
            [{"prompt": "<question text>", "context": "<background info or null>", "referenceAnswer": "<model answer>", "hints": "<helpful hint>", "exerciseType": "<Explanation|ReadingComprehension|FreeTextResponse>"}]
            """;
    }

    private static AIEvaluationResult ParseEvaluationResponse(string response)
    {
        string json = ExtractJson(response, '{', '}');

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        int score = root.TryGetProperty("score", out JsonElement scoreEl) && scoreEl.ValueKind == JsonValueKind.Number
            ? scoreEl.GetInt32() : 50;
        score = Math.Clamp(score, 0, 100);

        bool isPassing = root.TryGetProperty("isPassing", out JsonElement passEl) && passEl.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? passEl.GetBoolean() : score >= 60;

        string feedback = GetStringOrSerialize(root, "feedback") ?? "No feedback available.";
        string? correction = GetStringOrSerialize(root, "suggestedCorrection");
        string? breakdown = GetStringOrSerialize(root, "detailedBreakdown");

        return new AIEvaluationResult
        {
            Score = score,
            IsPassing = isPassing,
            Feedback = feedback,
            SuggestedCorrection = correction,
            DetailedBreakdown = breakdown
        };
    }

    private static List<GeneratedExercise> ParseGenerationResponse(string response, ExerciseGenerationRequest request)
    {
        string json = ExtractJson(response, '[', ']');

        using JsonDocument doc = JsonDocument.Parse(json);
        List<GeneratedExercise> exercises = new();

        foreach (JsonElement el in doc.RootElement.EnumerateArray())
        {
            string prompt = el.TryGetProperty("prompt", out JsonElement pEl) ? pEl.GetString() ?? "" : "";
            string? context = el.TryGetProperty("context", out JsonElement cEl) && cEl.ValueKind != JsonValueKind.Null ? cEl.GetString() : null;
            string refAnswer = el.TryGetProperty("referenceAnswer", out JsonElement rEl) ? rEl.GetString() ?? "" : "";
            string? hints = el.TryGetProperty("hints", out JsonElement hEl) && hEl.ValueKind != JsonValueKind.Null ? hEl.GetString() : null;

            ExerciseType exerciseType = ExerciseType.FreeTextResponse;
            if (el.TryGetProperty("exerciseType", out JsonElement tEl))
            {
                string typeName = tEl.GetString() ?? "";
                _ = Enum.TryParse(typeName, ignoreCase: true, out exerciseType);
            }

            if (!string.IsNullOrWhiteSpace(prompt) && !string.IsNullOrWhiteSpace(refAnswer))
            {
                exercises.Add(new GeneratedExercise
                {
                    Prompt = prompt,
                    Context = context,
                    ReferenceAnswer = refAnswer,
                    Hints = hints,
                    ExerciseType = exerciseType
                });
            }
        }

        return exercises.Count > 0 ? exercises : throw new InvalidOperationException("No valid exercises parsed from Claude response");
    }

    private static string ExtractJson(string text, char open, char close)
    {
        int start = text.IndexOf(open);
        if (start < 0)
            throw new JsonException($"No JSON found in response (looking for '{open}')");

        int depth = 0;
        for (int i = start; i < text.Length; i++)
        {
            if (text[i] == open) depth++;
            else if (text[i] == close) depth--;

            if (depth == 0)
                return text[start..(i + 1)];
        }

        throw new JsonException("Unbalanced JSON in response");
    }

    private static string? GetStringOrSerialize(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out JsonElement el))
            return null;

        return el.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => el.GetRawText()
        };
    }
}
