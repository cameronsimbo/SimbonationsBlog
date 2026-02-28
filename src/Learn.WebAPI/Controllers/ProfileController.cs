using Learn.Application.Profile.GetProfile;
using Learn.Application.Profile.GetProfile.Models;
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
}
