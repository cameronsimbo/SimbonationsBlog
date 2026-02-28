using Learn.Application.Exercises.Submit.Models;
using MediatR;

namespace Learn.Application.Exercises.Submit;

public record SubmitAnswerCommand : IRequest<ExerciseAttemptResultVm>
{
    public Guid ExerciseId { get; init; }
    public string UserAnswer { get; init; } = string.Empty;
    public int TimeTakenSeconds { get; init; }
}
