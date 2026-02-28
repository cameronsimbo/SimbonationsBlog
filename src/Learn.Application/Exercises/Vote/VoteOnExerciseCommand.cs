using Learn.Application.Exercises.Vote.Models;
using MediatR;

namespace Learn.Application.Exercises.Vote;

public record VoteOnExerciseCommand : IRequest<ExerciseVoteResultVm>
{
    public Guid ExerciseId { get; init; }
    public bool IsUpvote { get; init; }
}
