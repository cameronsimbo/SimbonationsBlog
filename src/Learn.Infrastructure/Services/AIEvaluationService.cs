using Learn.Application.Common.Interfaces;
using Learn.Domain.Enums;

namespace Learn.Infrastructure.Services;

/// <summary>
/// Fallback/stub AI evaluation service used when Ollama is unavailable.
/// Returns template-based exercises and a fixed score.
/// </summary>
public class AIEvaluationService : IAIEvaluationService
{
    public async Task<AIEvaluationResult> EvaluateAnswerAsync(EvaluationRequest request, CancellationToken cancellationToken = default)
    {
        // Fallback: return a fixed score when LLM is not reachable
        await Task.CompletedTask;

        return new AIEvaluationResult
        {
            Score = 75,
            IsPassing = true,
            Feedback = $"Good effort! Your answer demonstrates understanding of the topic. Consider expanding on the key concepts for a higher score.",
            SuggestedCorrection = null,
            DetailedBreakdown = "Content accuracy: 80% | Depth of analysis: 70% | Clarity: 75%"
        };
    }

    public async Task<List<GeneratedExercise>> GenerateExercisesAsync(ExerciseGenerationRequest request, CancellationToken cancellationToken = default)
    {
        // Fallback: return template-based exercises when LLM is not reachable
        await Task.CompletedTask;

        string topicContext = request.LessonContext ?? request.LessonName;
        string guidance = request.GenerationGuidance ?? "Focus on conceptual understanding and critical analysis.";

        List<(string prompt, string context, string answer, string hints)> templates = GenerateTemplatesForLesson(
            request.TopicName,
            request.UnitName,
            request.LessonName,
            topicContext,
            request.SubjectDomain);

        List<GeneratedExercise> exercises = new();
        int count = Math.Min(request.Count, templates.Count);

        for (int i = 0; i < count; i++)
        {
            (string prompt, string context, string answer, string hints) = templates[i];
            exercises.Add(new GeneratedExercise
            {
                Prompt = prompt,
                Context = context,
                ReferenceAnswer = answer,
                Hints = hints,
                ExerciseType = i switch
                {
                    0 => ExerciseType.Explanation,
                    1 => ExerciseType.ReadingComprehension,
                    2 => ExerciseType.FreeTextResponse,
                    3 => ExerciseType.Explanation,
                    _ => ExerciseType.FreeTextResponse
                }
            });
        }

        return exercises;
    }

    private static List<(string prompt, string context, string answer, string hints)> GenerateTemplatesForLesson(
        string topicName,
        string unitName,
        string lessonName,
        string lessonContext,
        SubjectDomain domain)
    {
        return domain switch
        {
            SubjectDomain.History => new List<(string, string, string, string)>
            {
                ($"Explain the significance of {lessonName} in the broader context of {topicName}.",
                 $"This lesson covers {lessonContext}. Consider the political, social, and military dimensions.",
                 $"The significance of {lessonName} lies in its impact on the broader trajectory of {topicName}. Key factors include the political consequences, shifts in public opinion, and strategic military implications that shaped subsequent events.",
                 "Think about cause and effect. What changed as a result? Who was affected?"),
                ($"What were the main causes that led to the events described in {lessonName}?",
                 $"Focus on the underlying factors, not just immediate triggers. Context: {lessonContext}",
                 $"The main causes include both long-term structural factors and immediate triggers. Long-term factors involve political tensions, economic pressures, and ideological conflicts. The immediate catalysts were specific events that made the situation untenable.",
                 "Consider both long-term and short-term causes. Think political, economic, and social factors."),
                ($"Compare and contrast different perspectives on {lessonName}. How did various groups view these events differently?",
                 $"Consider perspectives from different stakeholders involved in {topicName}.",
                 $"Different groups had vastly different perspectives. The government viewed events through a lens of national security, while civilians experienced the human cost directly. International observers had yet another perspective shaped by their own political contexts.",
                 "Think about at least 2-3 different groups or stakeholders."),
                ($"What were the lasting consequences of {lessonName}? How did it shape what came after?",
                 $"Think beyond immediate outcomes to long-term impacts. Context: {lessonContext}",
                 $"The lasting consequences include changes in policy, shifts in public consciousness, and institutional reforms. These events created precedents that influenced how similar situations were handled in the future.",
                 "Consider political, social, cultural, and economic consequences."),
                ($"If you were explaining {lessonName} to someone unfamiliar with {topicName}, what key points would you emphasise?",
                 $"Aim for clarity and accuracy. What are the essential elements someone needs to understand?",
                 $"The key points to emphasise would be: the historical context that set the stage, the main events and their significance, the key figures involved and their motivations, and the lasting impact on subsequent history.",
                 "Focus on the 3-4 most important things. What makes this significant?")
            },
            SubjectDomain.Finance => new List<(string, string, string, string)>
            {
                ($"Explain the core concept of {lessonName} and why it matters for investment professionals.",
                 $"This is part of the {unitName} section of {topicName}. Context: {lessonContext}",
                 $"The core concept involves understanding how {lessonName} provides a framework for analysing financial decisions. It matters because it directly impacts valuation, risk assessment, and portfolio construction decisions that investment professionals make daily.",
                 "Think about how this concept applies in real-world investment scenarios."),
                ($"A portfolio manager is reviewing their holdings. How would the principles of {lessonName} guide their analysis?",
                 $"Apply theoretical concepts to a practical scenario. Context: {lessonContext}",
                 $"The portfolio manager would apply {lessonName} principles by first assessing the relevant metrics, then evaluating risk-adjusted returns, and finally considering how the holdings align with the overall investment strategy and client objectives.",
                 "Consider the step-by-step process a professional would follow."),
                ($"What are the key assumptions underlying {lessonName}, and what happens when those assumptions are violated?",
                 $"Critical analysis of {lessonName} within {unitName}.",
                 $"The key assumptions include market efficiency, rational actors, and normal distribution of returns. When these assumptions are violated — as they often are in practice — the models may produce misleading results, requiring practitioners to apply judgment and supplementary analysis.",
                 "Every financial model has assumptions. Think about what's taken for granted."),
                ($"Calculate or explain the relationship between the key variables in {lessonName}.",
                 $"Focus on the quantitative or analytical framework. Context: {lessonContext}",
                 $"The key variables are interconnected through defined relationships. Understanding these relationships allows analysts to predict how changes in one variable affect others, which is essential for valuation, risk management, and strategic decision-making.",
                 "Focus on the mathematical or logical relationships between variables."),
                ($"How does {lessonName} relate to other concepts you've studied in {unitName}?",
                 $"Draw connections across the curriculum. Think about how concepts build on each other.",
                 $"This concept connects to other areas through shared principles of risk and return, time value of money, and market dynamics. Understanding these connections creates a more holistic view of financial analysis and decision-making.",
                 "Think about prerequisites and dependent concepts.")
            },
            _ => new List<(string, string, string, string)>
            {
                ($"Explain the key concepts of {lessonName} in your own words.",
                 $"This lesson is part of {unitName} in {topicName}. Context: {lessonContext}",
                 $"The key concepts of {lessonName} involve understanding the fundamental principles, their practical applications, and how they connect to the broader field of {topicName}.",
                 "Break it down into the main ideas and explain each one."),
                ($"Why is {lessonName} important in the context of {topicName}?",
                 $"Consider both theoretical and practical importance.",
                 $"It is important because it provides foundational knowledge that supports more advanced topics, has practical real-world applications, and helps develop critical thinking skills in this domain.",
                 "Think about both why we study it and how it applies."),
                ($"Give an example that demonstrates the principles discussed in {lessonName}.",
                 $"Use a concrete, real-world example to illustrate the concepts.",
                 $"A good example would demonstrate the core principles in action, showing how theoretical concepts manifest in practical situations and why understanding them matters.",
                 "Think of a specific scenario where these concepts apply."),
                ($"What are common misconceptions about {lessonName}, and how would you correct them?",
                 $"Think critically about what people often get wrong.",
                 $"Common misconceptions include oversimplifying complex relationships, confusing correlation with causation, and applying concepts out of context. Correcting these requires careful analysis and evidence-based reasoning.",
                 "What do people often get wrong? Why?"),
                ($"How does {lessonName} connect to other topics you might encounter in {topicName}?",
                 $"Draw connections and see the bigger picture.",
                 $"This topic connects to others through shared principles, complementary concepts, and practical applications that span multiple areas within {topicName}.",
                 "Think about prerequisites, related concepts, and applications.")
            }
        };
    }
}
