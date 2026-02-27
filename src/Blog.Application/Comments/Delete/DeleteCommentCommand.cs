using MediatR;

namespace Blog.Application.Comments.Delete;

public class DeleteCommentCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
