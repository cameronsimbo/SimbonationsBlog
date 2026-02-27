using Learn.Application.Common.Interfaces;
using Learn.Domain.Entities;
using MediatR;

namespace Learn.Application.Topics.Create;

public class CreateTopicCommandHandler : IRequestHandler<CreateTopicCommand, Guid>
{
    private readonly ILearnDbContext _db;

    public CreateTopicCommandHandler(ILearnDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(CreateTopicCommand command, CancellationToken cancellationToken)
    {
        Topic topic = Topic.Create(
            command.Name,
            command.Description,
            command.SubjectDomain,
            command.DifficultyLevel,
            command.IconUrl);

        _db.Topics.Add(topic);
        await _db.SaveChangesAsync(cancellationToken);

        return topic.Id;
    }
}
