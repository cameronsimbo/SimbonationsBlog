namespace Learn.Domain.Services;

public static class LevelThresholds
{
    private static readonly (int Level, int XPRequired, string Title)[] Levels = new[]
    {
        (1, 0, "Novice"),
        (2, 60, "Learner"),
        (3, 150, "Student"),
        (4, 300, "Scholar"),
        (5, 500, "Apprentice"),
        (6, 750, "Practitioner"),
        (7, 1100, "Adept"),
        (8, 1500, "Expert"),
        (9, 2000, "Master"),
        (10, 2750, "Grandmaster"),
        (11, 3500, "Sage"),
        (12, 4500, "Legend"),
        (13, 6000, "Mythic"),
        (14, 8000, "Transcendent"),
        (15, 10000, "Infinite")
    };

    public static int GetLevel(int totalXP)
    {
        int level = 1;

        for (int i = Levels.Length - 1; i >= 0; i--)
        {
            if (totalXP >= Levels[i].XPRequired)
            {
                level = Levels[i].Level;
                break;
            }
        }

        return level;
    }

    public static string GetTitle(int totalXP)
    {
        for (int i = Levels.Length - 1; i >= 0; i--)
        {
            if (totalXP >= Levels[i].XPRequired)
            {
                return Levels[i].Title;
            }
        }

        return "Novice";
    }

    public static int GetXPForNextLevel(int totalXP)
    {
        for (int i = 0; i < Levels.Length; i++)
        {
            if (totalXP < Levels[i].XPRequired)
            {
                return Levels[i].XPRequired;
            }
        }

        return Levels[^1].XPRequired;
    }

    public static double GetLevelProgress(int totalXP)
    {
        int currentLevelXP = 0;
        int nextLevelXP = Levels[1].XPRequired;

        for (int i = Levels.Length - 1; i >= 0; i--)
        {
            if (totalXP >= Levels[i].XPRequired)
            {
                currentLevelXP = Levels[i].XPRequired;
                nextLevelXP = i < Levels.Length - 1 ? Levels[i + 1].XPRequired : Levels[i].XPRequired;
                break;
            }
        }

        if (nextLevelXP == currentLevelXP)
        {
            return 1.0;
        }

        return (double)(totalXP - currentLevelXP) / (nextLevelXP - currentLevelXP);
    }

    public static int GetCrowns(int bestScore, int timesCompleted)
    {
        if (timesCompleted == 0)
        {
            return 0;
        }

        if (bestScore >= 95 && timesCompleted >= 3)
        {
            return 5;
        }

        if (bestScore >= 85 && timesCompleted >= 2)
        {
            return 4;
        }

        if (bestScore >= 75 && timesCompleted >= 2)
        {
            return 3;
        }

        if (bestScore >= 60)
        {
            return 2;
        }

        return 1;
    }
}
