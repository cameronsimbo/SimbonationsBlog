using Blog.Domain.Enums;
using System.Text.RegularExpressions;

namespace Blog.Domain.Entities;

public class Article : CreatedEntity<Article>
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? CoverImageUrl { get; set; }
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public DateTime? PublishedDate { get; set; }

    public Guid AuthorId { get; set; }
    public Author? Author { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public static Article Create(
        string title,
        string content,
        string? excerpt,
        string? coverImageUrl,
        Guid authorId,
        Guid categoryId)
    {
        Article article = new()
        {
            Title = title,
            Slug = GenerateSlug(title),
            Content = content,
            Excerpt = excerpt,
            CoverImageUrl = coverImageUrl,
            Status = ArticleStatus.Draft,
            AuthorId = authorId,
            CategoryId = categoryId
        };

        return article;
    }

    public void Publish()
    {
        Status = ArticleStatus.Published;
        PublishedDate = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = ArticleStatus.Archived;
    }

    private static string GenerateSlug(string title)
    {
        string slug = title.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }
}
