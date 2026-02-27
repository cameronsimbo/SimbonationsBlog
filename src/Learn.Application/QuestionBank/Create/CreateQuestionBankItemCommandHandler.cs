using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using Learn.Domain.Entities;
using MediatR;

namespace Learn.Application.QuestionBank.Create;

public class CreateQuestionBankItemCommandHandler : IRequestHandler<CreateQuestionBankItemCommand, Guid>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateQuestionBankItemCommandHandler(ILearnDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateQuestionBankItemCommand command, CancellationToken cancellationToken)
    {
        string userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User must be authenticated.");

        QuestionBankItem item = QuestionBankItem.Create(
            userId,
            command.SubjectDomain,
            command.ExerciseType,
            command.DifficultyLevel,
            command.Prompt,
            command.ReferenceAnswer,
            command.Context,
            command.Hints);

        _db.QuestionBankItems.Add(item);
        await _db.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
