using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Articles.Delete;

public class DeleteArticleCommandHandler : IRequestHandler<DeleteArticleCommand>
{
    private readonly IBlogDbContext _db;

    public DeleteArticleCommandHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteArticleCommand command, CancellationToken cancellationToken)
    {
        Article? article = await _db.Articles
            .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);

        if (article is null)
        {
            throw new NotFoundException(nameof(Article), command.Id);
        }

        _db.Articles.Remove(article);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
