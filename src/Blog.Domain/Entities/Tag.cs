using System.Text.RegularExpressions;

namespace Blog.Domain.Entities;

public class Tag : CreatedEntity<Tag>
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();

    public static Tag Create(string name)
    {
        return new Tag
        {
            Name = name,
            Slug = GenerateSlug(name)
        };
    }

    private static string GenerateSlug(string name)
    {
        string slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }
}
