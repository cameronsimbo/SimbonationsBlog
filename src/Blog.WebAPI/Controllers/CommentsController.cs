using Blog.Application.Comments.Create;
using Blog.Application.Comments.Delete;
using Blog.Application.Comments.GetByArticle;
using Blog.Application.Comments.Models;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebAPI.Controllers;

public class CommentsController : ApiControllerBase
{
    [HttpGet("article/{articleId:guid}")]
    public async Task<ActionResult<List<CommentVm>>> GetByArticle(Guid articleId)
    {
        return Ok(await Mediator.Send(new GetCommentsByArticleQuery { ArticleId = articleId }));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateCommentCommand command)
    {
        Guid id = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByArticle), new { articleId = command.ArticleId }, id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteCommentCommand { Id = id });
        return NoContent();
    }
}
