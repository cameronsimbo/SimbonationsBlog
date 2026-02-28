using Learn.Domain.Enums;

namespace Learn.Application.Exercises.GetByLesson.Models;

public record ExerciseVm
{
    public Guid Id { get; init; }
    public int OrderIndex { get; init; }
    public ExerciseType ExerciseType { get; init; }
    public DifficultyLevel DifficultyLevel { get; init; }
    public string Prompt { get; init; } = string.Empty;
    public string? Context { get; init; }
    public string? AudioUrl { get; init; }
    public string? Hints { get; init; }
    public int MaxScore { get; init; }
    public int UpvoteCount { get; init; }
    public int DownvoteCount { get; init; }
    public int VoteScore { get; init; }
    public bool? UserVote { get; init; }
}
