using Learn.Domain.Enums;

namespace Learn.Application.Common.Interfaces;

public interface IAIEvaluationService
{
    Task<AIEvaluationResult> EvaluateAnswerAsync(EvaluationRequest request, CancellationToken cancellationToken);
    Task<List<GeneratedExercise>> GenerateExercisesAsync(ExerciseGenerationRequest request, CancellationToken cancellationToken);
}

public record EvaluationRequest
{
    public string UserAnswer { get; init; } = string.Empty;
    public ExerciseType ExerciseType { get; init; }
    public string Prompt { get; init; } = string.Empty;
    public string ReferenceAnswer { get; init; } = string.Empty;
    public SubjectDomain SubjectDomain { get; init; }
    public DifficultyLevel DifficultyLevel { get; init; }
}

public record AIEvaluationResult
{
    public int Score { get; init; }
    public bool IsPassing { get; init; }
    public string Feedback { get; init; } = string.Empty;
    public string? SuggestedCorrection { get; init; }
    public string? DetailedBreakdown { get; init; }
}

public record ExerciseGenerationRequest
{
    public SubjectDomain SubjectDomain { get; init; }
    public DifficultyLevel DifficultyLevel { get; init; }
    public ExerciseType ExerciseType { get; init; }
    public int Count { get; init; } = 5;
    public string? SeedPrompt { get; init; }
    public string? SeedReferenceAnswer { get; init; }
    public string TopicName { get; init; } = string.Empty;
    public string UnitName { get; init; } = string.Empty;
    public string LessonName { get; init; } = string.Empty;
    public string? KeyConcepts { get; init; }
    public string? GenerationGuidance { get; init; }
    public string? LessonContext { get; init; }
}

public record GeneratedExercise
{
    public string Prompt { get; init; } = string.Empty;
    public string? Context { get; init; }
    public string ReferenceAnswer { get; init; } = string.Empty;
    public string? Hints { get; init; }
    public ExerciseType ExerciseType { get; init; }
}
