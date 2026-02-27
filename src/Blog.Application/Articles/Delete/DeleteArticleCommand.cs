using MediatR;

namespace Blog.Application.Articles.Delete;

public record DeleteArticleCommand : IRequest
{
    public Guid Id { get; init; }
}
