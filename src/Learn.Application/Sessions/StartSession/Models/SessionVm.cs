namespace Learn.Application.Sessions.StartSession.Models;

public record SessionVm
{
    public Guid EnrollmentId { get; init; }
    public Guid TopicId { get; init; }
    public string TopicName { get; init; } = string.Empty;
    public Guid CurrentLessonId { get; init; }
    public string CurrentLessonName { get; init; } = string.Empty;
    public string CurrentUnitName { get; init; } = string.Empty;
    public int UnitIndex { get; init; }
    public int LessonIndex { get; init; }
    public List<SessionExerciseVm> Exercises { get; init; } = new();
    public int NewExerciseCount { get; init; }
    public int ReviewExerciseCount { get; init; }
}

public record SessionExerciseVm
{
    public Guid ExerciseId { get; init; }
    public string Prompt { get; init; } = string.Empty;
    public string? Context { get; init; }
    public string? Hints { get; init; }
    public int ExerciseType { get; init; }
    public int DifficultyLevel { get; init; }
    public bool IsReview { get; init; }
    public bool IsInterleaved { get; init; }
}
