using Blog.Application.Tags.Models;
using MediatR;

namespace Blog.Application.Tags.GetAll;

public class GetTagsQuery : IRequest<List<TagVm>>
{
}
