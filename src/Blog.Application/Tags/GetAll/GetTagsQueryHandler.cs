using Blog.Application.Common.Interfaces;
using Blog.Application.Tags.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Tags.GetAll;

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, List<TagVm>>
{
    private readonly IBlogDbContext _db;

    public GetTagsQueryHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<List<TagVm>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        return await _db.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagVm
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug
            })
            .ToListAsync(cancellationToken);
    }
}
