using Blog.Application.Comments.Models;
using MediatR;

namespace Blog.Application.Comments.GetByArticle;

public class GetCommentsByArticleQuery : IRequest<List<CommentVm>>
{
    public Guid ArticleId { get; set; }
}
