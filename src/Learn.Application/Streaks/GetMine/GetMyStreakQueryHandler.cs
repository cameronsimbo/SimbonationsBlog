using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using Learn.Application.Streaks.GetMine.Models;
using Learn.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Streaks.GetMine;

public class GetMyStreakQueryHandler : IRequestHandler<GetMyStreakQuery, StreakVm>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetMyStreakQueryHandler(ILearnDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<StreakVm> Handle(GetMyStreakQuery request, CancellationToken cancellationToken)
    {
        string userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User must be authenticated.");

        UserStreak? streak = await _db.UserStreaks
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (streak is null)
        {
            return new StreakVm
            {
                CurrentStreak = 0,
                LongestStreak = 0,
                StreakFreezeCount = 2
            };
        }

        return new StreakVm
        {
            CurrentStreak = streak.CurrentStreak,
            LongestStreak = streak.LongestStreak,
            LastActivityDate = streak.LastActivityDate,
            StreakFreezeCount = streak.StreakFreezeCount,
            StreakFreezeUsedDate = streak.StreakFreezeUsedDate
        };
    }
}
