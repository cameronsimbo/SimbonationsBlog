using Learn.Application.Progress.GetDashboard;
using Learn.Application.Progress.GetDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learn.WebAPI.Controllers;

[Authorize]
public class DashboardController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardVm>> Get(CancellationToken cancellationToken = default)
    {
        DashboardVm result = await Mediator.Send(new GetDashboardQuery(), cancellationToken);
        return Ok(result);
    }
}
