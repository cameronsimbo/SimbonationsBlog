using Blog.Domain.Enums;

namespace Blog.Application.Comments.Models;

public class CommentVm
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public CommentStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? ParentCommentId { get; set; }
    public List<CommentVm> Replies { get; set; } = new();
}
