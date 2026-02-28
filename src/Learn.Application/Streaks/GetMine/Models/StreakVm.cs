namespace Learn.Application.Streaks.GetMine.Models;

public record StreakVm
{
    public int CurrentStreak { get; init; }
    public int LongestStreak { get; init; }
    public DateTime? LastActivityDate { get; init; }
    public int StreakFreezeCount { get; init; }
    public DateTime? StreakFreezeUsedDate { get; init; }
}
