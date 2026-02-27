using MediatR;

namespace Learn.Application.Topics.Get;

public record GetTopicQuery : IRequest<TopicDetailVm>
{
    public Guid Id { get; init; }
}

public record TopicDetailVm
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Domain.Enums.SubjectDomain SubjectDomain { get; init; }
    public string? IconUrl { get; init; }
    public Domain.Enums.DifficultyLevel DifficultyLevel { get; init; }
    public bool IsPublished { get; init; }
    public int TotalUnits { get; init; }
    public List<UnitSummaryVm> Units { get; init; } = new();
}

public record UnitSummaryVm
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public int TotalLessons { get; init; }
}
