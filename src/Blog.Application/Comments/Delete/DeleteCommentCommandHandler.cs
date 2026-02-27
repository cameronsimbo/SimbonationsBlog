using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Comments.Delete;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Unit>
{
    private readonly IBlogDbContext _db;

    public DeleteCommentCommandHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        Comment entity = await _db.Comments
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Comment), request.Id);

        _db.Comments.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
