using MediatR;

namespace Blog.Application.Tags.Upsert;

public class UpsertTagCommand : IRequest<Guid>
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
