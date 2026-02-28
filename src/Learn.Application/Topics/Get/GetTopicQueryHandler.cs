using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using Learn.Application.Topics.Get.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Topics.Get;

public class GetTopicQueryHandler : IRequestHandler<GetTopicQuery, TopicDetailVm>
{
    private readonly ILearnDbContext _db;

    public GetTopicQueryHandler(ILearnDbContext db)
    {
        _db = db;
    }

    public async Task<TopicDetailVm> Handle(GetTopicQuery request, CancellationToken cancellationToken)
    {
        TopicDetailVm? topic = await _db.Topics
            .Where(t => t.Id == request.Id)
            .Select(t => new TopicDetailVm
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                SubjectDomain = t.SubjectDomain,
                IconUrl = t.IconUrl,
                DifficultyLevel = t.DifficultyLevel,
                IsPublished = t.IsPublished,
                TotalUnits = t.TotalUnits,
                Units = t.Units
                    .OrderBy(u => u.OrderIndex)
                    .Select(u => new UnitSummaryVm
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Description = u.Description,
                        OrderIndex = u.OrderIndex,
                        TotalLessons = u.TotalLessons
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (topic is null)
        {
            throw new NotFoundException(nameof(Domain.Entities.Topic), request.Id);
        }

        return topic;
    }
}
