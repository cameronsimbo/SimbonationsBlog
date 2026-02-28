using Learn.Application.Exercises.Generate;
using Learn.Application.Exercises.GetByLesson;
using Learn.Application.Exercises.GetByLesson.Models;
using Learn.Application.Exercises.Submit;
using Learn.Application.Exercises.Submit.Models;
using Learn.Application.Exercises.Vote;
using Learn.Application.Exercises.Vote.Models;
using Learn.WebAPI.Controllers.Models;
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

    [HttpPost("generate")]
    public async Task<ActionResult<List<ExerciseVm>>> Generate(
        GenerateExercisesCommand command,
        CancellationToken cancellationToken = default)
    {
        List<ExerciseVm> result = await Mediator.Send(command, cancellationToken);
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

    [HttpPost("{id:guid}/vote")]
    public async Task<ActionResult<ExerciseVoteResultVm>> Vote(
        Guid id,
        VoteRequest request,
        CancellationToken cancellationToken = default)
    {
        ExerciseVoteResultVm result = await Mediator.Send(
            new VoteOnExerciseCommand { ExerciseId = id, IsUpvote = request.IsUpvote },
            cancellationToken);

        return Ok(result);
    }
}
