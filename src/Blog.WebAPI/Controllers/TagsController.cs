using Blog.Application.Tags.GetAll;
using Blog.Application.Tags.Models;
using Blog.Application.Tags.Upsert;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebAPI.Controllers;

public class TagsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TagVm>>> GetAll()
    {
        return Ok(await Mediator.Send(new GetTagsQuery()));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(UpsertTagCommand command)
    {
        command.Id = null;
        Guid id = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpsertTagCommand command)
    {
        command.Id = id;
        await Mediator.Send(command);
        return NoContent();
    }
}
