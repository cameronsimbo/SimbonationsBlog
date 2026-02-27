using MediatR;

namespace Learn.Application.Progress.GetLearningPath;

public record GetLearningPathQuery : IRequest<LearningPathVm>
{
    public Guid TopicId { get; init; }
}

public record LearningPathVm
{
    public Guid TopicId { get; init; }
    public string TopicName { get; init; } = string.Empty;
    public int CurrentUnitIndex { get; init; }
    public int CurrentLessonIndex { get; init; }
    public int TotalXPEarned { get; init; }
    public List<PathUnitVm> Units { get; init; } = new();
}

public record PathUnitVm
{
    public Guid UnitId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public bool IsCurrent { get; init; }
    public bool IsCompleted { get; init; }
    public bool IsLocked { get; init; }
    public List<PathLessonVm> Lessons { get; init; } = new();
}

public record PathLessonVm
{
    public Guid LessonId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public bool IsCurrent { get; init; }
    public bool IsCompleted { get; init; }
    public bool IsLocked { get; init; }
    public int Crowns { get; init; }
    public int BestScore { get; init; }
}
