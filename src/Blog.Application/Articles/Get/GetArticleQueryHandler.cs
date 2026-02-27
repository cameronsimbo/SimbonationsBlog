using Blog.Application.Articles.Models;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Application.Tags.Models;
using Blog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Articles.Get;

public class GetArticleQueryHandler : IRequestHandler<GetArticleQuery, ArticleDetailVm>
{
    private readonly IBlogDbContext _db;

    public GetArticleQueryHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<ArticleDetailVm> Handle(GetArticleQuery request, CancellationToken cancellationToken)
    {
        Article? article = await _db.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (article is null)
        {
            throw new NotFoundException(nameof(Article), request.Id);
        }

        return new ArticleDetailVm
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Content = article.Content,
            Excerpt = article.Excerpt,
            CoverImageUrl = article.CoverImageUrl,
            Status = article.Status,
            PublishedDate = article.PublishedDate,
            CreatedDate = article.CreatedDate,
            AuthorId = article.AuthorId,
            AuthorName = article.Author?.DisplayName ?? "",
            CategoryId = article.CategoryId,
            CategoryName = article.Category?.Name ?? "",
            Tags = article.ArticleTags
                .Where(at => at.Tag is not null)
                .Select(at => new TagVm { Id = at.Tag!.Id, Name = at.Tag.Name, Slug = at.Tag.Slug })
                .ToList()
        };
    }
}
