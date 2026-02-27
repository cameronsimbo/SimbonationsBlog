using MediatR;

namespace Learn.Application.Leaderboards.GetWeekly;

public record GetWeeklyLeaderboardQuery : IRequest<LeaderboardResultVm>
{
    public int Top { get; init; } = 50;
}

public record LeaderboardResultVm
{
    public List<LeaderboardEntryVm> Entries { get; init; } = new();
    public LeaderboardEntryVm? CurrentUserEntry { get; init; }
}

public record LeaderboardEntryVm
{
    public int Rank { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public int WeeklyXP { get; init; }
}
