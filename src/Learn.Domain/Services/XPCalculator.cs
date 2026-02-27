namespace Learn.Domain.Services;

public static class XPCalculator
{
    private const int BaseXPPerExercise = 10;
    private const int PerfectScoreBonus = 5;
    private const int StreakBonusPerDay = 2;
    private const int MaxStreakBonus = 10;

    public static int Calculate(int score, int currentStreak)
    {
        double scoreMultiplier = score / 100.0;
        int baseXP = (int)(BaseXPPerExercise * scoreMultiplier);

        int perfectBonus = score == 100 ? PerfectScoreBonus : 0;

        int streakBonus = Math.Min(currentStreak * StreakBonusPerDay, MaxStreakBonus);

        return baseXP + perfectBonus + streakBonus;
    }
}
