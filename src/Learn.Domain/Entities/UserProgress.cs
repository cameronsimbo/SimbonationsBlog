namespace Learn.Domain.Entities;

public class UserProgress : CreatedEntity<UserProgress>
{
    public string UserId { get; set; } = string.Empty;
    public Guid TopicId { get; set; }
    public Guid LessonId { get; set; }
    public int ExercisesCompleted { get; set; }
    public int ExercisesTotal { get; set; }
    public int BestScore { get; set; }
    public double AverageScore { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int MasteryLevel { get; set; }
    public int TimesCompleted { get; set; }

    public Topic Topic { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;

    public static UserProgress Create(
        string userId,
        Guid topicId,
        Guid lessonId,
        int exercisesTotal)
    {
        return new UserProgress
        {
            UserId = userId,
            TopicId = topicId,
            LessonId = lessonId,
            ExercisesCompleted = 0,
            ExercisesTotal = exercisesTotal,
            BestScore = 0,
            AverageScore = 0,
            IsCompleted = false
        };
    }

    public void RecordAttempt(int score)
    {
        ExercisesCompleted++;

        if (score > BestScore)
        {
            BestScore = score;
        }

        AverageScore = ((AverageScore * (ExercisesCompleted - 1)) + score) / ExercisesCompleted;

        if (ExercisesCompleted >= ExercisesTotal)
        {
            IsCompleted = true;
            CompletedDate = DateTime.UtcNow;
            TimesCompleted++;
            MasteryLevel = Domain.Services.LevelThresholds.GetCrowns(BestScore, TimesCompleted);
        }
    }
}
