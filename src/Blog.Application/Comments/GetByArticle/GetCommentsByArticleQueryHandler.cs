using Blog.Application.Comments.Models;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Comments.GetByArticle;

public class GetCommentsByArticleQueryHandler : IRequestHandler<GetCommentsByArticleQuery, List<CommentVm>>
{
    private readonly IBlogDbContext _db;

    public GetCommentsByArticleQueryHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<List<CommentVm>> Handle(GetCommentsByArticleQuery request, CancellationToken cancellationToken)
    {
        List<CommentVm> comments = await _db.Comments
            .AsNoTracking()
            .Where(c => c.ArticleId == request.ArticleId && c.Status == CommentStatus.Approved)
            .OrderBy(c => c.CreatedDate)
            .Select(c => new CommentVm
            {
                Id = c.Id,
                AuthorName = c.AuthorName,
                Content = c.Content,
                Status = c.Status,
                CreatedDate = c.CreatedDate,
                ParentCommentId = c.ParentCommentId
            })
            .ToListAsync(cancellationToken);

        // Build tree: top-level comments with nested replies
        Dictionary<Guid, CommentVm> lookup = comments.ToDictionary(c => c.Id);
        List<CommentVm> roots = new();

        foreach (CommentVm comment in comments)
        {
            if (comment.ParentCommentId is null)
            {
                roots.Add(comment);
            }
            else if (lookup.TryGetValue(comment.ParentCommentId.Value, out CommentVm? parent))
            {
                parent.Replies.Add(comment);
            }
        }

        return roots;
    }
}
