using Blog.Application.Authors.Models;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Authors.Get;

public class GetAuthorQueryHandler : IRequestHandler<GetAuthorQuery, AuthorVm>
{
    private readonly IBlogDbContext _db;

    public GetAuthorQueryHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<AuthorVm> Handle(GetAuthorQuery request, CancellationToken cancellationToken)
    {
        Author? author = await _db.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (author is null)
        {
            throw new NotFoundException(nameof(Author), request.Id);
        }

        return new AuthorVm
        {
            Id = author.Id,
            DisplayName = author.DisplayName,
            Email = author.Email,
            Bio = author.Bio,
            AvatarUrl = author.AvatarUrl
        };
    }
}
