using Learn.Application.Exercises.GetByLesson;
using Learn.Application.Exercises.Submit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learn.WebAPI.Controllers;

[Authorize]
public class ExercisesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ExerciseVm>>> GetByLesson(
        [FromQuery] Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        List<ExerciseVm> result = await Mediator.Send(
            new GetExercisesByLessonQuery { LessonId = lessonId },
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("submit")]
    public async Task<ActionResult<ExerciseAttemptResultVm>> Submit(
        SubmitAnswerCommand command,
        CancellationToken cancellationToken = default)
    {
        ExerciseAttemptResultVm result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
