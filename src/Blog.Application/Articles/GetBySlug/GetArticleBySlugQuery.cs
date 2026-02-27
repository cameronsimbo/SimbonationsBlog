using Blog.Application.Articles.Models;
using MediatR;

namespace Blog.Application.Articles.GetBySlug;

public record GetArticleBySlugQuery : IRequest<ArticleDetailVm>
{
    public string Slug { get; init; } = string.Empty;
}
