using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Authors.Upsert;

public class UpsertAuthorCommandHandler : IRequestHandler<UpsertAuthorCommand, Guid>
{
    private readonly IBlogDbContext _db;

    public UpsertAuthorCommandHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(UpsertAuthorCommand command, CancellationToken cancellationToken)
    {
        Author? author;

        if (command.Id.HasValue)
        {
            author = await _db.Authors
                .FirstOrDefaultAsync(a => a.Id == command.Id.Value, cancellationToken);

            if (author is null)
            {
                throw new NotFoundException(nameof(Author), command.Id.Value);
            }

            author.DisplayName = command.DisplayName;
            author.Email = command.Email;
            author.Bio = command.Bio;
            author.AvatarUrl = command.AvatarUrl;
            author.ModifiedDate = DateTime.UtcNow;
        }
        else
        {
            author = new Author
            {
                DisplayName = command.DisplayName,
                Email = command.Email,
                Bio = command.Bio,
                AvatarUrl = command.AvatarUrl
            };
            _db.Authors.Add(author);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return author.Id;
    }
}
