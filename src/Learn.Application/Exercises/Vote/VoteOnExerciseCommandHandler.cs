using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using Learn.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Exercises.Vote;

public class VoteOnExerciseCommandHandler : IRequestHandler<VoteOnExerciseCommand, ExerciseVoteResultVm>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;

    public VoteOnExerciseCommandHandler(ILearnDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ExerciseVoteResultVm> Handle(VoteOnExerciseCommand command, CancellationToken cancellationToken)
    {
        string userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User must be authenticated.");

        Exercise? exercise = await _db.Exercises
            .FirstOrDefaultAsync(e => e.Id == command.ExerciseId, cancellationToken);

        if (exercise is null)
        {
            throw new NotFoundException(nameof(Exercise), command.ExerciseId);
        }

        ExerciseVote? existingVote = await _db.ExerciseVotes
            .FirstOrDefaultAsync(v => v.ExerciseId == command.ExerciseId && v.UserId == userId, cancellationToken);

        bool? resultingVote;

        if (existingVote is null)
        {
            // New vote
            ExerciseVote vote = ExerciseVote.Create(command.ExerciseId, userId, command.IsUpvote);
            _db.ExerciseVotes.Add(vote);

            if (command.IsUpvote)
            {
                exercise.IncrementUpvote();
            }
            else
            {
                exercise.IncrementDownvote();
            }

            resultingVote = command.IsUpvote;
        }
        else if (existingVote.IsUpvote == command.IsUpvote)
        {
            // Same vote again — toggle off (remove)
            _db.ExerciseVotes.Remove(existingVote);

            if (command.IsUpvote)
            {
                exercise.DecrementUpvote();
            }
            else
            {
                exercise.DecrementDownvote();
            }

            resultingVote = null;
        }
        else
        {
            // Flip vote direction
            existingVote.ChangeVote(command.IsUpvote);

            if (command.IsUpvote)
            {
                exercise.DecrementDownvote();
                exercise.IncrementUpvote();
            }
            else
            {
                exercise.DecrementUpvote();
                exercise.IncrementDownvote();
            }

            resultingVote = command.IsUpvote;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new ExerciseVoteResultVm
        {
            UpvoteCount = exercise.UpvoteCount,
            DownvoteCount = exercise.DownvoteCount,
            VoteScore = exercise.VoteScore,
            UserVote = resultingVote
        };
    }
}
