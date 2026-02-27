using Blog.Application.Articles.Models;
using MediatR;

namespace Blog.Application.Articles.Get;

public record GetArticleQuery : IRequest<ArticleDetailVm>
{
    public Guid Id { get; init; }
}
