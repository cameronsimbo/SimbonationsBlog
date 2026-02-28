using Learn.Application.Topics.Create;
using Learn.Application.Topics.Enroll;
using Learn.Application.Topics.Enroll.Models;
using Learn.Application.Topics.Get;
using Learn.Application.Topics.Get.Models;
using Learn.Application.Topics.GetAll;
using Learn.Application.Topics.GetAll.Models;
using Learn.Application.Progress.GetLearningPath;
using Learn.Application.Progress.GetLearningPath.Models;
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

    [HttpPost("{id:guid}/enroll")]
    [Authorize]
    public async Task<ActionResult<EnrollmentResultVm>> Enroll(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        EnrollmentResultVm result = await Mediator.Send(
            new EnrollInTopicCommand { TopicId = id },
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/path")]
    [Authorize]
    public async Task<ActionResult<LearningPathVm>> GetPath(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        LearningPathVm result = await Mediator.Send(
            new GetLearningPathQuery { TopicId = id },
            cancellationToken);

        return Ok(result);
    }
}
