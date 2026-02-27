using Blog.Application.Tags.Models;
using Blog.Domain.Enums;

namespace Blog.Application.Articles.Models;

public record ArticleDetailVm
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? CoverImageUrl { get; init; }
    public ArticleStatus Status { get; init; }
    public DateTime? PublishedDate { get; init; }
    public DateTime CreatedDate { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public List<TagVm> Tags { get; init; } = new();
}
