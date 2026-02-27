using Learn.Application.QuestionBank.Create;
using Learn.Application.QuestionBank.GetMine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learn.WebAPI.Controllers;

[Authorize]
public class QuestionBankController : ApiControllerBase
{
    [HttpGet("mine")]
    public async Task<ActionResult<List<QuestionBankItemVm>>> GetMine(
        CancellationToken cancellationToken = default)
    {
        List<QuestionBankItemVm> result = await Mediator.Send(
            new GetMyQuestionBankQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        CreateQuestionBankItemCommand command,
        CancellationToken cancellationToken = default)
    {
        Guid id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMine), id);
    }
}
