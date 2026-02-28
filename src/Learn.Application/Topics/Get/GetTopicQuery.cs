using Learn.Application.Topics.Get.Models;
using MediatR;

namespace Learn.Application.Topics.Get;

public record GetTopicQuery : IRequest<TopicDetailVm>
{
    public Guid Id { get; init; }
}
