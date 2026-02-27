using MediatR;

namespace Blog.Application.Articles.Create;

public record CreateArticleCommand : IRequest<Guid>
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? CoverImageUrl { get; init; }
    public Guid AuthorId { get; init; }
    public Guid CategoryId { get; init; }
    public List<Guid> TagIds { get; init; } = new();
}
