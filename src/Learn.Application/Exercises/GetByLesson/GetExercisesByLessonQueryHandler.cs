using Learn.Application.Common.Interfaces;
using Learn.Application.Exercises.GetByLesson.Models;
using Learn.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Exercises.GetByLesson;

public class GetExercisesByLessonQueryHandler : IRequestHandler<GetExercisesByLessonQuery, List<ExerciseVm>>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetExercisesByLessonQueryHandler(ILearnDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<ExerciseVm>> Handle(GetExercisesByLessonQuery request, CancellationToken cancellationToken)
    {
        string? userId = _currentUser.UserId;

        List<Exercise> exercises = await _db.Exercises
            .Include(e => e.Votes)
            .Where(e => e.LessonId == request.LessonId)
            .Where(e => (e.UpvoteCount - e.DownvoteCount) >= -3)
            .OrderBy(e => e.OrderIndex)
            .ThenByDescending(e => e.UpvoteCount - e.DownvoteCount)
            .ToListAsync(cancellationToken);

        List<ExerciseVm> result = exercises.Select(e =>
        {
            ExerciseVote? userVote = string.IsNullOrEmpty(userId)
                ? null
                : e.Votes.FirstOrDefault(v => v.UserId == userId);

            return new ExerciseVm
            {
                Id = e.Id,
                OrderIndex = e.OrderIndex,
                ExerciseType = e.ExerciseType,
                DifficultyLevel = e.DifficultyLevel,
                Prompt = e.Prompt,
                Context = e.Context,
                AudioUrl = e.AudioUrl,
                Hints = e.Hints,
                MaxScore = e.MaxScore,
                UpvoteCount = e.UpvoteCount,
                DownvoteCount = e.DownvoteCount,
                VoteScore = e.VoteScore,
                UserVote = userVote is null ? null : userVote.IsUpvote
            };
        }).ToList();

        return result;
    }
}
