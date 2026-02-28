using Learn.Application.Streaks.GetMine;
using Learn.Application.Streaks.GetMine.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learn.WebAPI.Controllers;

[Authorize]
public class StreaksController : ApiControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<StreakVm>> GetMine(CancellationToken cancellationToken = default)
    {
        StreakVm result = await Mediator.Send(new GetMyStreakQuery(), cancellationToken);
        return Ok(result);
    }
}
