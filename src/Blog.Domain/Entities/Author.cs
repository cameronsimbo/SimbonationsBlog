namespace Blog.Domain.Entities;

public class Author : CreatedEntity<Author>
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }

    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
