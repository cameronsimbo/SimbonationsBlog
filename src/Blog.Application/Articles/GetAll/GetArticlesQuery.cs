using Blog.Application.Articles.Models;
using Blog.Application.Common.Models;
using Blog.Domain.Enums;
using MediatR;

namespace Blog.Application.Articles.GetAll;

public record GetArticlesQuery : IRequest<PaginatedList<ArticleSummaryVm>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? CategoryId { get; init; }
    public Guid? TagId { get; init; }
    public ArticleStatus? Status { get; init; }
    public string? SearchTerm { get; init; }
}
