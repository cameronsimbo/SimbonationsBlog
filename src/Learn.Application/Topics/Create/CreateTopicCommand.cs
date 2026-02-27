using Learn.Domain.Enums;
using MediatR;

namespace Learn.Application.Topics.Create;

public record CreateTopicCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public SubjectDomain SubjectDomain { get; init; }
    public DifficultyLevel DifficultyLevel { get; init; }
    public string? IconUrl { get; init; }
}
