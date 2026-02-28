namespace Learn.Application.Exercises.Submit.Models;

public record ExerciseAttemptResultVm
{
    public Guid AttemptId { get; init; }
    public int Score { get; init; }
    public bool IsPassing { get; init; }
    public string Feedback { get; init; } = string.Empty;
    public string? SuggestedCorrection { get; init; }
    public string? DetailedBreakdown { get; init; }
    public int XPEarned { get; init; }
    public int DailySubmissionsRemaining { get; init; }
    public bool IsLessonComplete { get; init; }
    public int UpvoteCount { get; init; }
    public int DownvoteCount { get; init; }
    public bool? UserVote { get; init; }
}
