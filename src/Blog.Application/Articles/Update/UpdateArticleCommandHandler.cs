using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Articles.Update;

public class UpdateArticleCommandHandler : IRequestHandler<UpdateArticleCommand>
{
    private readonly IBlogDbContext _db;

    public UpdateArticleCommandHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task Handle(UpdateArticleCommand command, CancellationToken cancellationToken)
    {
        Article? article = await _db.Articles
            .Include(a => a.ArticleTags)
            .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);

        if (article is null)
        {
            throw new NotFoundException(nameof(Article), command.Id);
        }

        article.Title = command.Title;
        article.Content = command.Content;
        article.Excerpt = command.Excerpt;
        article.CoverImageUrl = command.CoverImageUrl;
        article.CategoryId = command.CategoryId;
        article.ModifiedDate = DateTime.UtcNow;

        List<ArticleTag> existingTags = article.ArticleTags.ToList();
        foreach (ArticleTag existingTag in existingTags)
        {
            _db.ArticleTags.Remove(existingTag);
        }

        foreach (Guid tagId in command.TagIds)
        {
            _db.ArticleTags.Add(new ArticleTag { ArticleId = article.Id, TagId = tagId });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
