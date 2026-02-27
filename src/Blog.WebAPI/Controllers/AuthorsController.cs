using Blog.Application.Authors.Get;
using Blog.Application.Authors.GetAll;
using Blog.Application.Authors.Models;
using Blog.Application.Authors.Upsert;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebAPI.Controllers;

public class AuthorsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuthorVm>>> GetAll()
    {
        return Ok(await Mediator.Send(new GetAuthorsQuery()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuthorVm>> Get(Guid id)
    {
        return Ok(await Mediator.Send(new GetAuthorQuery { Id = id }));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(UpsertAuthorCommand command)
    {
        command.Id = null;
        Guid id = await Mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpsertAuthorCommand command)
    {
        command.Id = id;
        await Mediator.Send(command);
        return NoContent();
    }
}
