namespace Learn.Domain.Entities;

public class ReviewItem : CreatedEntity<ReviewItem>
{
    public string UserId { get; set; } = string.Empty;
    public Guid ExerciseId { get; set; }
    public DateTime NextReviewDate { get; set; }
    public int Interval { get; set; } = 1;

    // FSRS-lite fields
    public double Stability { get; set; } = 1.0;   // Days to 90% retention
    public double Difficulty { get; set; } = 5.0;  // 1.0 (easy) to 10.0 (hard)

    // Kept for EF compatibility — no longer written to
    public double EaseFactor { get; set; } = 2.5;
    public int RepetitionCount { get; set; }

    public Exercise Exercise { get; set; } = null!;

    public static ReviewItem Create(string userId, Guid exerciseId) => new()
    {
        UserId = userId,
        ExerciseId = exerciseId,
        NextReviewDate = DateTime.UtcNow.AddDays(1),
        Interval = 1,
        Stability = 1.0,
        Difficulty = 5.0,
        EaseFactor = 2.5,
        RepetitionCount = 0
    };

    /// <summary>
    /// FSRS-lite spaced repetition algorithm.
    /// </summary>
    public void RecordReview(int score)
    {
        int grade = MapScoreToGrade(score);

        double daysSinceReview = Math.Max(1, (DateTime.UtcNow - (ModifiedDate ?? CreatedDate)).TotalDays);
        double retrievability = Math.Exp(Math.Log(0.9) * daysSinceReview / Stability);

        if (grade >= 3)
        {
            // Successful recall: stability grows (desirable difficulty effect)
            Stability *= 1 + 2.5 * (1 - retrievability) * (11 - Difficulty) / 10.0;
        }
        else
        {
            // Lapse: reduce stability by half, not a full reset like SM-2
            Stability = Math.Max(1.0, Stability * 0.5);
        }

        Difficulty = Math.Clamp(Difficulty + 0.1 - (grade - 3) * 0.08, 1.0, 10.0);

        Interval = (int)Math.Ceiling(Stability);
        NextReviewDate = DateTime.UtcNow.AddDays(Interval);
    }

    public bool IsDueForReview()
    {
        return DateTime.UtcNow.Date >= NextReviewDate.Date;
    }

    private static int MapScoreToGrade(int score)
    {
        return score switch
        {
            >= 95 => 5,
            >= 80 => 4,
            >= 60 => 3,
            >= 40 => 2,
            >= 20 => 1,
            _ => 0
        };
    }
}
