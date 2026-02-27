using Blog.Application.Categories.GetAll;
using Blog.Application.Categories.Models;
using Blog.Application.Categories.Upsert;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebAPI.Controllers;

public class CategoriesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryVm>>> GetAll()
    {
        return Ok(await Mediator.Send(new GetCategoriesQuery()));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(UpsertCategoryCommand command)
    {
        command.Id = null;
        Guid id = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpsertCategoryCommand command)
    {
        command.Id = id;
        await Mediator.Send(command);
        return NoContent();
    }
}
