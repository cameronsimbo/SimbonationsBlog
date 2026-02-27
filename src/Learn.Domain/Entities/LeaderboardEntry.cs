namespace Learn.Domain.Entities;

public class LeaderboardEntry : BaseEntity<LeaderboardEntry>
{
    public string UserId { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }
    public int WeeklyXP { get; set; }

    public static LeaderboardEntry Create(string userId, DateTime weekStartDate)
    {
        return new LeaderboardEntry
        {
            UserId = userId,
            WeekStartDate = weekStartDate,
            WeeklyXP = 0
        };
    }

    public void AddXP(int xp)
    {
        WeeklyXP += xp;
    }

    public static DateTime GetCurrentWeekStart(DateTime utcNow)
    {
        int daysUntilMonday = ((int)utcNow.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return utcNow.Date.AddDays(-daysUntilMonday);
    }
}
