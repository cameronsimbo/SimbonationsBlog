using Learn.Application.Common.Interfaces;
using Learn.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Leaderboards.GetWeekly;

public class GetWeeklyLeaderboardQueryHandler : IRequestHandler<GetWeeklyLeaderboardQuery, LeaderboardResultVm>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetWeeklyLeaderboardQueryHandler(ILearnDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<LeaderboardResultVm> Handle(GetWeeklyLeaderboardQuery request, CancellationToken cancellationToken)
    {
        DateTime weekStart = LeaderboardEntry.GetCurrentWeekStart(DateTime.UtcNow);

        List<LeaderboardEntry> entries = await _db.LeaderboardEntries
            .Where(l => l.WeekStartDate == weekStart)
            .OrderByDescending(l => l.WeeklyXP)
            .Take(request.Top)
            .ToListAsync(cancellationToken);

        List<LeaderboardEntryVm> rankedEntries = entries
            .Select((e, index) => new LeaderboardEntryVm
            {
                Rank = index + 1,
                UserId = e.UserId,
                DisplayName = e.UserId,
                WeeklyXP = e.WeeklyXP
            })
            .ToList();

        LeaderboardEntryVm? currentUserEntry = null;
        if (_currentUser.UserId is not null)
        {
            currentUserEntry = rankedEntries.FirstOrDefault(e => e.UserId == _currentUser.UserId);

            if (currentUserEntry is null)
            {
                LeaderboardEntry? userEntry = await _db.LeaderboardEntries
                    .FirstOrDefaultAsync(l => l.UserId == _currentUser.UserId && l.WeekStartDate == weekStart, cancellationToken);

                if (userEntry is not null)
                {
                    int rank = await _db.LeaderboardEntries
                        .CountAsync(l => l.WeekStartDate == weekStart && l.WeeklyXP > userEntry.WeeklyXP, cancellationToken) + 1;

                    currentUserEntry = new LeaderboardEntryVm
                    {
                        Rank = rank,
                        UserId = userEntry.UserId,
                        DisplayName = userEntry.UserId,
                        WeeklyXP = userEntry.WeeklyXP
                    };
                }
            }
        }

        return new LeaderboardResultVm
        {
            Entries = rankedEntries,
            CurrentUserEntry = currentUserEntry
        };
    }
}
