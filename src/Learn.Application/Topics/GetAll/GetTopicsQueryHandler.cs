using Learn.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Topics.GetAll;

public class GetTopicsQueryHandler : IRequestHandler<GetTopicsQuery, List<TopicVm>>
{
    private readonly ILearnDbContext _db;

    public GetTopicsQueryHandler(ILearnDbContext db)
    {
        _db = db;
    }

    public async Task<List<TopicVm>> Handle(GetTopicsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Topic> query = _db.Topics.AsQueryable();

        if (request.PublishedOnly)
        {
            query = query.Where(t => t.IsPublished == true);
        }

        if (request.SubjectDomain.HasValue)
        {
            query = query.Where(t => t.SubjectDomain == request.SubjectDomain.Value);
        }

        List<TopicVm> topics = await query
            .OrderBy(t => t.Name)
            .Select(t => new TopicVm
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                SubjectDomain = t.SubjectDomain,
                IconUrl = t.IconUrl,
                DifficultyLevel = t.DifficultyLevel,
                IsPublished = t.IsPublished,
                TotalUnits = t.TotalUnits
            })
            .ToListAsync(cancellationToken);

        return topics;
    }
}
