using Learn.Application.Progress.GetLearningPath.Models;
using MediatR;

namespace Learn.Application.Progress.GetLearningPath;

public record GetLearningPathQuery : IRequest<LearningPathVm>
{
    public Guid TopicId { get; init; }
}
