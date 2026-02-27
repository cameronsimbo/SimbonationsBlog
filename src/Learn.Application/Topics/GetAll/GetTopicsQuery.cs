using Learn.Domain.Enums;
using MediatR;

namespace Learn.Application.Topics.GetAll;

public record GetTopicsQuery : IRequest<List<TopicVm>>
{
    public SubjectDomain? SubjectDomain { get; init; }
    public bool PublishedOnly { get; init; } = true;
}

public record TopicVm
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public SubjectDomain SubjectDomain { get; init; }
    public string? IconUrl { get; init; }
    public DifficultyLevel DifficultyLevel { get; init; }
    public bool IsPublished { get; init; }
    public int TotalUnits { get; init; }
}
