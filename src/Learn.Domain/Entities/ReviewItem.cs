namespace Learn.Domain.Entities;

public class ReviewItem : CreatedEntity<ReviewItem>
{
    public string UserId { get; set; } = string.Empty;
    public Guid ExerciseId { get; set; }
    public DateTime NextReviewDate { get; set; }
    public int Interval { get; set; } = 1;
    public double EaseFactor { get; set; } = 2.5;
    public int RepetitionCount { get; set; }

    public Exercise Exercise { get; set; } = null!;

    public static ReviewItem Create(string userId, Guid exerciseId)
    {
        return new ReviewItem
        {
            UserId = userId,
            ExerciseId = exerciseId,
            NextReviewDate = DateTime.UtcNow.AddDays(1),
            Interval = 1,
            EaseFactor = 2.5,
            RepetitionCount = 0
        };
    }

    /// <summary>
    /// SM-2 spaced repetition algorithm.
    /// Quality: 0-5 scale mapped from score (0-100).
    /// </summary>
    public void RecordReview(int score)
    {
        int quality = MapScoreToQuality(score);

        if (quality >= 3)
        {
            RepetitionCount++;

            if (RepetitionCount == 1)
            {
                Interval = 1;
            }
            else if (RepetitionCount == 2)
            {
                Interval = 6;
            }
            else
            {
                Interval = (int)Math.Round(Interval * EaseFactor);
            }
        }
        else
        {
            RepetitionCount = 0;
            Interval = 1;
        }

        EaseFactor = EaseFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));

        if (EaseFactor < 1.3)
        {
            EaseFactor = 1.3;
        }

        NextReviewDate = DateTime.UtcNow.AddDays(Interval);
    }

    public bool IsDueForReview()
    {
        return DateTime.UtcNow.Date >= NextReviewDate.Date;
    }

    private static int MapScoreToQuality(int score)
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
