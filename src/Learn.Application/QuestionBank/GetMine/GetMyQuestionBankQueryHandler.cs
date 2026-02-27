using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.QuestionBank.GetMine;

public class GetMyQuestionBankQueryHandler : IRequestHandler<GetMyQuestionBankQuery, List<QuestionBankItemVm>>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetMyQuestionBankQueryHandler(ILearnDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<QuestionBankItemVm>> Handle(GetMyQuestionBankQuery request, CancellationToken cancellationToken)
    {
        string userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User must be authenticated.");

        IQueryable<Domain.Entities.QuestionBankItem> query = _db.QuestionBankItems
            .Where(q => q.UserId == userId);

        if (request.SubjectDomain.HasValue)
        {
            query = query.Where(q => q.SubjectDomain == request.SubjectDomain.Value);
        }

        if (request.ExerciseType.HasValue)
        {
            query = query.Where(q => q.ExerciseType == request.ExerciseType.Value);
        }

        if (request.DifficultyLevel.HasValue)
        {
            query = query.Where(q => q.DifficultyLevel == request.DifficultyLevel.Value);
        }

        List<QuestionBankItemVm> items = await query
            .OrderByDescending(q => q.CreatedDate)
            .Select(q => new QuestionBankItemVm
            {
                Id = q.Id,
                SubjectDomain = q.SubjectDomain,
                ExerciseType = q.ExerciseType,
                DifficultyLevel = q.DifficultyLevel,
                Prompt = q.Prompt,
                Context = q.Context,
                ReferenceAnswer = q.ReferenceAnswer,
                Hints = q.Hints,
                UsageCount = q.UsageCount
            })
            .ToListAsync(cancellationToken);

        return items;
    }
}
