using Blog.Application.Authors.Models;
using Blog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Authors.GetAll;

public class GetAuthorsQueryHandler : IRequestHandler<GetAuthorsQuery, List<AuthorVm>>
{
    private readonly IBlogDbContext _db;

    public GetAuthorsQueryHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<List<AuthorVm>> Handle(GetAuthorsQuery request, CancellationToken cancellationToken)
    {
        return await _db.Authors
            .AsNoTracking()
            .OrderBy(a => a.DisplayName)
            .Select(a => new AuthorVm
            {
                Id = a.Id,
                DisplayName = a.DisplayName,
                Email = a.Email,
                Bio = a.Bio,
                AvatarUrl = a.AvatarUrl
            })
            .ToListAsync(cancellationToken);
    }
}
