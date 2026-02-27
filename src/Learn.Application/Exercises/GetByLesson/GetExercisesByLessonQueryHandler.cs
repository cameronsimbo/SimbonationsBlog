using Learn.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Exercises.GetByLesson;

public class GetExercisesByLessonQueryHandler : IRequestHandler<GetExercisesByLessonQuery, List<ExerciseVm>>
{
    private readonly ILearnDbContext _db;

    public GetExercisesByLessonQueryHandler(ILearnDbContext db)
    {
        _db = db;
    }

    public async Task<List<ExerciseVm>> Handle(GetExercisesByLessonQuery request, CancellationToken cancellationToken)
    {
        List<ExerciseVm> exercises = await _db.Exercises
            .Where(e => e.LessonId == request.LessonId)
            .OrderBy(e => e.OrderIndex)
            .Select(e => new ExerciseVm
            {
                Id = e.Id,
                OrderIndex = e.OrderIndex,
                ExerciseType = e.ExerciseType,
                DifficultyLevel = e.DifficultyLevel,
                Prompt = e.Prompt,
                Context = e.Context,
                AudioUrl = e.AudioUrl,
                Hints = e.Hints,
                MaxScore = e.MaxScore
            })
            .ToListAsync(cancellationToken);

        return exercises;
    }
}
