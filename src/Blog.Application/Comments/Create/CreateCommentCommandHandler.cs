using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Blog.Domain.Enums;
using MediatR;

namespace Blog.Application.Comments.Create;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Guid>
{
    private readonly IBlogDbContext _db;

    public CreateCommentCommandHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        Comment entity = new()
        {
            ArticleId = request.ArticleId,
            AuthorName = request.AuthorName,
            AuthorEmail = request.AuthorEmail,
            Content = request.Content,
            Status = CommentStatus.Pending,
            ParentCommentId = request.ParentCommentId
        };

        _db.Comments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
