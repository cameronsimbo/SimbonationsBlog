using MediatR;

namespace Learn.Application.Lessons.GetByUnit;

public record GetLessonsByUnitQuery : IRequest<List<LessonVm>>
{
    public Guid UnitId { get; init; }
}

public record LessonVm
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public int ExerciseCount { get; init; }
    public int EstimatedMinutes { get; init; }
}
