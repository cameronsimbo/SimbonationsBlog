using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using MediatR;

namespace Blog.Application.Articles.Create;

public class CreateArticleCommandHandler : IRequestHandler<CreateArticleCommand, Guid>
{
    private readonly IBlogDbContext _db;

    public CreateArticleCommandHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(CreateArticleCommand command, CancellationToken cancellationToken)
    {
        Article article = Article.Create(
            command.Title,
            command.Content,
            command.Excerpt,
            command.CoverImageUrl,
            command.AuthorId,
            command.CategoryId);

        _db.Articles.Add(article);

        foreach (Guid tagId in command.TagIds)
        {
            _db.ArticleTags.Add(new ArticleTag { ArticleId = article.Id, TagId = tagId });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return article.Id;
    }
}
