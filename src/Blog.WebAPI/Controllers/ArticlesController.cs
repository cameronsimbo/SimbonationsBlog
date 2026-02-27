using Blog.Application.Articles.Create;
using Blog.Application.Articles.Delete;
using Blog.Application.Articles.Get;
using Blog.Application.Articles.GetAll;
using Blog.Application.Articles.GetBySlug;
using Blog.Application.Articles.Models;
using Blog.Application.Articles.Publish;
using Blog.Application.Articles.Update;
using Blog.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebAPI.Controllers;

public class ArticlesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedList<ArticleSummaryVm>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? tagId = null,
        [FromQuery] string? searchTerm = null)
    {
        GetArticlesQuery query = new()
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            CategoryId = categoryId,
            TagId = tagId,
            SearchTerm = searchTerm
        };

        return Ok(await Mediator.Send(query));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticleDetailVm>> Get(Guid id)
    {
        return Ok(await Mediator.Send(new GetArticleQuery { Id = id }));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<ArticleDetailVm>> GetBySlug(string slug)
    {
        return Ok(await Mediator.Send(new GetArticleBySlugQuery { Slug = slug }));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateArticleCommand command)
    {
        Guid id = await Mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateArticleCommand command)
    {
        command.Id = id;
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteArticleCommand { Id = id });
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(Guid id)
    {
        await Mediator.Send(new PublishArticleCommand { Id = id });
        return NoContent();
    }
}
