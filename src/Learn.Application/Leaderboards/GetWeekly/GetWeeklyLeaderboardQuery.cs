using Learn.Application.Leaderboards.GetWeekly.Models;
using MediatR;

namespace Learn.Application.Leaderboards.GetWeekly;

public record GetWeeklyLeaderboardQuery : IRequest<LeaderboardResultVm>
{
    public int Top { get; init; } = 50;
}
