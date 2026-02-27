using MediatR;

namespace Learn.Application.Exercises.Vote;

public record VoteOnExerciseCommand : IRequest<ExerciseVoteResultVm>
{
    public Guid ExerciseId { get; init; }
    public bool IsUpvote { get; init; }
}

public record ExerciseVoteResultVm
{
    public int UpvoteCount { get; init; }
    public int DownvoteCount { get; init; }
    public int VoteScore { get; init; }
    public bool? UserVote { get; init; }
}
