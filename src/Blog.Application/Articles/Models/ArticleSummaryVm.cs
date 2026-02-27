namespace Blog.Application.Articles.Models;

public record ArticleSummaryVm
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? CoverImageUrl { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public DateTime? PublishedDate { get; init; }
    public List<string> Tags { get; init; } = new();
}
