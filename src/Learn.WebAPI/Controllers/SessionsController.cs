using Learn.Application.Sessions.CompleteSession;
using Learn.Application.Sessions.StartSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learn.WebAPI.Controllers;

[Authorize]
public class SessionsController : ApiControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<SessionVm>> Start(
        StartSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        SessionVm result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("complete")]
    public async Task<ActionResult<SessionCompleteVm>> Complete(
        CompleteSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        SessionCompleteVm result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
