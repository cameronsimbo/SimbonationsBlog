using FluentValidation;
using FluentValidation.Results;
using Learn.Application.Common.Interfaces;
using Learn.Domain.Entities;
using MediatR;

namespace Learn.Application.Topics.Create;

public class CreateTopicCommandHandler : IRequestHandler<CreateTopicCommand, Guid>
{
    private readonly ILearnDbContext _db;
    private readonly IAIEvaluationService _aiService;

    public CreateTopicCommandHandler(ILearnDbContext db, IAIEvaluationService aiService)
    {
        _db = db;
        _aiService = aiService;
    }

    public async Task<Guid> Handle(CreateTopicCommand command, CancellationToken cancellationToken)
    {
        Topic topic = Topic.Create(
            command.Name,
            command.Description,
            command.SubjectDomain,
            command.DifficultyLevel,
            command.IconUrl);

        TopicValidationResult validation = await _aiService.ValidateTopicAsync(
            new TopicValidationRequest { TopicName = command.Name, Description = command.Description },
            cancellationToken);

        if (validation.IsValid == false)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Name", validation.RejectionReason
                    ?? "This topic is not suitable for the learning platform.")
            });
        }

        _db.Topics.Add(topic);
        await _db.SaveChangesAsync(cancellationToken);

        return topic.Id;
    }
}
