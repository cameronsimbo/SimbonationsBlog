using Learn.Application.QuestionBank.GetMine.Models;
using Learn.Domain.Enums;
using MediatR;

namespace Learn.Application.QuestionBank.GetMine;

public record GetMyQuestionBankQuery : IRequest<List<QuestionBankItemVm>>
{
    public SubjectDomain? SubjectDomain { get; init; }
    public ExerciseType? ExerciseType { get; init; }
    public DifficultyLevel? DifficultyLevel { get; init; }
}
