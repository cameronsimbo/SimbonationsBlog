using Learn.Application.Topics.GetAll.Models;
using Learn.Domain.Enums;
using MediatR;

namespace Learn.Application.Topics.GetAll;

public record GetTopicsQuery : IRequest<List<TopicVm>>
{
    public SubjectDomain? SubjectDomain { get; init; }
    public bool PublishedOnly { get; init; } = true;
}
