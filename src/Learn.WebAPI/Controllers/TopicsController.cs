using Learn.Application.Topics.Create;
using Learn.Application.Topics.Get;
using Learn.Application.Topics.GetAll;
using Learn.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learn.WebAPI.Controllers;

public class TopicsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TopicVm>>> GetAll(
        [FromQuery] SubjectDomain? subjectDomain,
        [FromQuery] bool publishedOnly = true,
        CancellationToken cancellationToken = default)
    {
        List<TopicVm> result = await Mediator.Send(
            new GetTopicsQuery { SubjectDomain = subjectDomain, PublishedOnly = publishedOnly },
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TopicDetailVm>> Get(Guid id, CancellationToken cancellationToken = default)
    {
        TopicDetailVm result = await Mediator.Send(new GetTopicQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Create(
        CreateTopicCommand command,
        CancellationToken cancellationToken = default)
    {
        Guid id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }
}
