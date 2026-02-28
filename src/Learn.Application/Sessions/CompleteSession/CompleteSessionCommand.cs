using Learn.Application.Sessions.CompleteSession.Models;
using MediatR;

namespace Learn.Application.Sessions.CompleteSession;

public record CompleteSessionCommand : IRequest<SessionCompleteVm>
{
    public Guid TopicId { get; init; }
    public List<ExerciseResultDto> Results { get; init; } = new();
}
