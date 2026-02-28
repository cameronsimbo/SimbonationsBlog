using Learn.Application.Sessions.StartSession.Models;
using MediatR;

namespace Learn.Application.Sessions.StartSession;

public record StartSessionCommand : IRequest<SessionVm>
{
    public Guid TopicId { get; init; }
}
