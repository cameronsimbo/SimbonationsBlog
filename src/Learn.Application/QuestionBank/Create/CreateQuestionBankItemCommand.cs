using Learn.Domain.Enums;
using MediatR;

namespace Learn.Application.QuestionBank.Create;

public record CreateQuestionBankItemCommand : IRequest<Guid>
{
    public SubjectDomain SubjectDomain { get; init; }
    public ExerciseType ExerciseType { get; init; }
    public DifficultyLevel DifficultyLevel { get; init; }
    public string Prompt { get; init; } = string.Empty;
    public string? Context { get; init; }
    public string ReferenceAnswer { get; init; } = string.Empty;
    public string? Hints { get; init; }
}
