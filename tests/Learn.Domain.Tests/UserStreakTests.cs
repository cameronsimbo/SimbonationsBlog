using FluentAssertions;
using Learn.Domain.Entities;
using Xunit;

namespace Learn.Domain.Tests;

public class UserStreakTests
{
    [Fact]
    public void RecordActivity_FirstActivity_SetsStreakToOne()
    {
        UserStreak streak = UserStreak.Create("user-1");

        streak.RecordActivity(DateTime.UtcNow);

        streak.CurrentStreak.Should().Be(1);
        streak.LongestStreak.Should().Be(1);
    }

    [Fact]
    public void RecordActivity_ConsecutiveDay_IncrementsStreak()
    {
        UserStreak streak = UserStreak.Create("user-1");
        DateTime yesterday = DateTime.UtcNow.Date.AddDays(-1);
        streak.RecordActivity(yesterday);

        streak.RecordActivity(DateTime.UtcNow);

        streak.CurrentStreak.Should().Be(2);
    }

    [Fact]
    public void RecordActivity_SameDay_DoesNotIncrementStreak()
    {
        UserStreak streak = UserStreak.Create("user-1");
        DateTime today = DateTime.UtcNow;
        streak.RecordActivity(today);

        streak.RecordActivity(today);

        streak.CurrentStreak.Should().Be(1);
    }
}
