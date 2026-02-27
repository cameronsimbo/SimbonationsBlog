using Learn.Application.Lessons.GetByUnit;
using Microsoft.AspNetCore.Mvc;

namespace Learn.WebAPI.Controllers;

public class LessonsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LessonVm>>> GetByUnit(
        [FromQuery] Guid unitId,
        CancellationToken cancellationToken = default)
    {
        List<LessonVm> result = await Mediator.Send(
            new GetLessonsByUnitQuery { UnitId = unitId },
            cancellationToken);

        return Ok(result);
    }
}
