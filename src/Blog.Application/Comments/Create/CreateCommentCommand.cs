using MediatR;

namespace Blog.Application.Comments.Create;

public class CreateCommentCommand : IRequest<Guid>
{
    public Guid ArticleId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
}
