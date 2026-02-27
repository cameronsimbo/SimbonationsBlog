using MediatR;

namespace Blog.Application.Authors.Upsert;

public record UpsertAuthorCommand : IRequest<Guid>
{
    public Guid? Id { get; set; }
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
}
