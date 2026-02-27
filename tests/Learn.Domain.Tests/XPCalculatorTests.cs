using FluentAssertions;
using Learn.Domain.Services;
using Xunit;

namespace Learn.Domain.Tests;

public class XPCalculatorTests
{
    [Fact]
    public void Calculate_PerfectScore_NoStreak_ReturnsBaseXPPlusPerfectBonus()
    {
        int result = XPCalculator.Calculate(100, 0);

        result.Should().Be(15); // 10 * (100/100) + 5 perfect + 0 streak
    }

    [Fact]
    public void Calculate_ZeroScore_NoStreak_ReturnsZero()
    {
        int result = XPCalculator.Calculate(0, 0);

        result.Should().Be(0); // 10 * 0 + 0 + 0
    }

    [Fact]
    public void Calculate_PerfectScore_WithStreak_IncludesStreakBonus()
    {
        int result = XPCalculator.Calculate(100, 3);

        result.Should().Be(21); // 10 + 5 + min(6, 10)
    }

    [Fact]
    public void Calculate_StreakBonus_CappedAtTen()
    {
        int result = XPCalculator.Calculate(100, 10);

        result.Should().Be(25); // 10 + 5 + 10 (capped)
    }

    [Fact]
    public void Calculate_PartialScore_CalculatesCorrectly()
    {
        int result = XPCalculator.Calculate(50, 2);

        result.Should().Be(9); // 10 * 0.5 + 0 + min(4, 10) = 5 + 0 + 4
    }
}
