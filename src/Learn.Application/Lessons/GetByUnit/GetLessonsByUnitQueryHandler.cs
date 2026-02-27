using Learn.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Lessons.GetByUnit;

public class GetLessonsByUnitQueryHandler : IRequestHandler<GetLessonsByUnitQuery, List<LessonVm>>
{
    private readonly ILearnDbContext _db;

    public GetLessonsByUnitQueryHandler(ILearnDbContext db)
    {
        _db = db;
    }

    public async Task<List<LessonVm>> Handle(GetLessonsByUnitQuery request, CancellationToken cancellationToken)
    {
        List<LessonVm> lessons = await _db.Lessons
            .Where(l => l.UnitId == request.UnitId)
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonVm
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                OrderIndex = l.OrderIndex,
                ExerciseCount = l.ExerciseCount,
                EstimatedMinutes = l.EstimatedMinutes
            })
            .ToListAsync(cancellationToken);

        return lessons;
    }
}
