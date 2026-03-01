using Learn.Application.Common.Interfaces;
using Learn.Application.Profile.GetProfile;
using Learn.Application.Profile.GetProfile.Models;
using Learn.Application.Profile.TestAI;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learn.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<UserProfileVm>> Get(CancellationToken cancellationToken)
    {
        UserProfileVm profile = await _mediator.Send(new GetProfileQuery(), cancellationToken);
        return Ok(profile);
    }

    [HttpPost("test-ai")]
    public async Task<ActionResult<TestConnectionResult>> TestAI(
        [FromBody] TestAICommand command, CancellationToken cancellationToken)
    {
        TestConnectionResult result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
