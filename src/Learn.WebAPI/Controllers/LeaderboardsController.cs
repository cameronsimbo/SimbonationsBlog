using Learn.Application.Leaderboards.GetWeekly;
using Microsoft.AspNetCore.Mvc;

namespace Learn.WebAPI.Controllers;

public class LeaderboardsController : ApiControllerBase
{
    [HttpGet("weekly")]
    public async Task<ActionResult<LeaderboardResultVm>> GetWeekly(
        [FromQuery] int top = 50,
        CancellationToken cancellationToken = default)
    {
        LeaderboardResultVm result = await Mediator.Send(
            new GetWeeklyLeaderboardQuery { Top = top },
            cancellationToken);

        return Ok(result);
    }
}
