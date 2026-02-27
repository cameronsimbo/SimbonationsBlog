namespace Learn.Domain.Entities;

public class UserStreak : CreatedEntity<UserStreak>
{
    public string UserId { get; set; } = string.Empty;
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public int StreakFreezeCount { get; set; } = 2;
    public DateTime? StreakFreezeUsedDate { get; set; }

    public static UserStreak Create(string userId)
    {
        return new UserStreak
        {
            UserId = userId,
            CurrentStreak = 0,
            LongestStreak = 0,
            StreakFreezeCount = 2
        };
    }

    public void RecordActivity(DateTime utcNow)
    {
        DateTime today = utcNow.Date;

        if (LastActivityDate.HasValue && LastActivityDate.Value.Date == today)
        {
            return;
        }

        if (LastActivityDate.HasValue)
        {
            DateTime yesterday = today.AddDays(-1);
            DateTime lastDate = LastActivityDate.Value.Date;

            if (lastDate == yesterday)
            {
                CurrentStreak++;
            }
            else if (lastDate < yesterday)
            {
                bool freezeUsedToday = StreakFreezeUsedDate.HasValue
                    && StreakFreezeUsedDate.Value.Date == yesterday;

                if (freezeUsedToday == false && StreakFreezeCount > 0 && lastDate == yesterday.AddDays(-1))
                {
                    StreakFreezeCount--;
                    StreakFreezeUsedDate = yesterday;
                    CurrentStreak++;
                }
                else if (freezeUsedToday)
                {
                    CurrentStreak++;
                }
                else
                {
                    CurrentStreak = 1;
                }
            }
        }
        else
        {
            CurrentStreak = 1;
        }

        if (CurrentStreak > LongestStreak)
        {
            LongestStreak = CurrentStreak;
        }

        LastActivityDate = today;
    }

    public bool CanUseFreeze()
    {
        return StreakFreezeCount > 0;
    }

    public void UseFreeze(DateTime utcNow)
    {
        if (StreakFreezeCount <= 0)
        {
            throw new InvalidOperationException("No streak freezes available.");
        }

        StreakFreezeCount--;
        StreakFreezeUsedDate = utcNow.Date;
    }
}
