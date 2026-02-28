namespace Learn.Domain.Services;

public static class SessionEngine
{
    public const int ExercisesPerSession = 5;
    public const double ReviewRatio = 0.3;

    /// <summary>
    /// Returns (newCount, reviewCount, interleavedCount).
    /// interleavedCount is 1 when past-lesson exercises are available, else 0.
    /// </summary>
    public static (int newCount, int reviewCount, int interleavedCount) CalculateSessionMix(
        int availableReviewItems, bool hasPastLessonExercises)
    {
        int interleavedCount = hasPastLessonExercises ? 1 : 0;
        int slotsForReview = ExercisesPerSession - interleavedCount;
        int reviewCount = Math.Min(availableReviewItems, (int)Math.Floor(slotsForReview * ReviewRatio));
        int newCount = ExercisesPerSession - reviewCount - interleavedCount;
        return (newCount, reviewCount, interleavedCount);
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
