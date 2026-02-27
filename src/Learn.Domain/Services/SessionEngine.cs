namespace Learn.Domain.Services;

public static class SessionEngine
{
    public const int ExercisesPerSession = 5;
    public const double ReviewRatio = 0.3;

    public static (int newCount, int reviewCount) CalculateSessionMix(int availableReviewItems)
    {
        int reviewCount = (int)Math.Floor(ExercisesPerSession * ReviewRatio);

        if (availableReviewItems < reviewCount)
        {
            reviewCount = availableReviewItems;
        }

        int newCount = ExercisesPerSession - reviewCount;

        return (newCount, reviewCount);
    }

    public static int CalculateSessionXPBonus(int totalScore, int exerciseCount)
    {
        if (exerciseCount == 0)
        {
            return 0;
        }

        int averageScore = totalScore / exerciseCount;

        if (averageScore >= 90)
        {
            return 10;
        }

        if (averageScore >= 70)
        {
            return 5;
        }

        return 0;
    }
}
