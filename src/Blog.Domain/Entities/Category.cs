using System.Text.RegularExpressions;

namespace Blog.Domain.Entities;

public class Category : CreatedEntity<Category>
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Article> Articles { get; set; } = new List<Article>();

    public static Category Create(string name, string? description)
    {
        return new Category
        {
            Name = name,
            Slug = GenerateSlug(name),
            Description = description
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
