using Learn.Domain.Enums;
using MediatR;

namespace Learn.Application.QuestionBank.GetMine;

public record GetMyQuestionBankQuery : IRequest<List<QuestionBankItemVm>>
{
    public SubjectDomain? SubjectDomain { get; init; }
    public ExerciseType? ExerciseType { get; init; }
    public DifficultyLevel? DifficultyLevel { get; init; }
}

public record QuestionBankItemVm
{
    public Guid Id { get; init; }
    public SubjectDomain SubjectDomain { get; init; }
    public ExerciseType ExerciseType { get; init; }
    public DifficultyLevel DifficultyLevel { get; init; }
    public string Prompt { get; init; } = string.Empty;
    public string? Context { get; init; }
    public string ReferenceAnswer { get; init; } = string.Empty;
    public string? Hints { get; init; }
    public int UsageCount { get; init; }
}
