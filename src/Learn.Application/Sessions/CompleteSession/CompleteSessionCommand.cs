using MediatR;

namespace Learn.Application.Sessions.CompleteSession;

public record CompleteSessionCommand : IRequest<SessionCompleteVm>
{
    public Guid TopicId { get; init; }
    public List<ExerciseResultDto> Results { get; init; } = new();
}

public record ExerciseResultDto
{
    public Guid ExerciseId { get; init; }
    public int Score { get; init; }
    public bool IsPassing { get; init; }
    public int XPEarned { get; init; }
    public bool IsReview { get; init; }
}

public record SessionCompleteVm
{
    public int TotalXPEarned { get; init; }
    public int SessionBonus { get; init; }
    public int ExercisesCompleted { get; init; }
    public int AverageScore { get; init; }
    public bool LessonComplete { get; init; }
    public bool UnitAdvanced { get; init; }
    public int NewLevel { get; init; }
    public string LevelTitle { get; init; } = string.Empty;
    public double LevelProgress { get; init; }
    public int CurrentStreak { get; init; }
    public int ReviewItemsCreated { get; init; }
}
