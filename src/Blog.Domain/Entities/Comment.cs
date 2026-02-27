using Blog.Domain.Enums;

namespace Blog.Domain.Entities;

public class Comment : CreatedEntity<Comment>
{
    public Guid ArticleId { get; set; }
    public Article? Article { get; set; }

    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public CommentStatus Status { get; set; } = CommentStatus.Pending;

    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
