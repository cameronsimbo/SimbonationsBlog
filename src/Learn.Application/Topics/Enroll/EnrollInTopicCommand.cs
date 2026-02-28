using Learn.Application.Topics.Enroll.Models;
using MediatR;

namespace Learn.Application.Topics.Enroll;

public record EnrollInTopicCommand : IRequest<EnrollmentResultVm>
{
    public Guid TopicId { get; init; }
}
