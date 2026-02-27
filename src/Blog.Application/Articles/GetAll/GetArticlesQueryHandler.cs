using Blog.Application.Articles.Models;
using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Models;
using Blog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Articles.GetAll;

public class GetArticlesQueryHandler : IRequestHandler<GetArticlesQuery, PaginatedList<ArticleSummaryVm>>
{
    private readonly IBlogDbContext _db;

    public GetArticlesQueryHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedList<ArticleSummaryVm>> Handle(GetArticlesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Article> query = _db.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .AsNoTracking();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == request.CategoryId.Value);
        }

        if (request.TagId.HasValue)
        {
            query = query.Where(a => a.ArticleTags.Any(at => at.TagId == request.TagId.Value));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        if (string.IsNullOrWhiteSpace(request.SearchTerm) == false)
        {
            string term = request.SearchTerm.Trim().ToLower();
            query = query.Where(a => a.Title.ToLower().Contains(term) || (a.Excerpt != null && a.Excerpt.ToLower().Contains(term)));
        }

        IQueryable<ArticleSummaryVm> projected = query
            .OrderByDescending(a => a.PublishedDate ?? a.CreatedDate)
            .Select(a => new ArticleSummaryVm
            {
                Id = a.Id,
                Title = a.Title,
                Slug = a.Slug,
                Excerpt = a.Excerpt,
                CoverImageUrl = a.CoverImageUrl,
                AuthorName = a.Author != null ? a.Author.DisplayName : "",
                CategoryName = a.Category != null ? a.Category.Name : "",
                PublishedDate = a.PublishedDate,
                Tags = a.ArticleTags.Select(at => at.Tag != null ? at.Tag.Name : "").ToList()
            });

        return await PaginatedList<ArticleSummaryVm>.CreateAsync(
            projected,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
