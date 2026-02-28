namespace Learn.Application.Exercises.Vote.Models;

public record ExerciseVoteResultVm
{
    public int UpvoteCount { get; init; }
    public int DownvoteCount { get; init; }
    public int VoteScore { get; init; }
    public bool? UserVote { get; init; }
}
