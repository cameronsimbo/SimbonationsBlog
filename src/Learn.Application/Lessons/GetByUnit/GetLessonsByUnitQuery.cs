using Learn.Application.Lessons.GetByUnit.Models;
using MediatR;

namespace Learn.Application.Lessons.GetByUnit;

public record GetLessonsByUnitQuery : IRequest<List<LessonVm>>
{
    public Guid UnitId { get; init; }
}
