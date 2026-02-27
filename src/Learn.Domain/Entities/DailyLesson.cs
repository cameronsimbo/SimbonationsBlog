namespace Learn.Domain.Entities;

public class DailyLesson : CreatedEntity<DailyLesson>
{
    public string UserId { get; set; } = string.Empty;
    public Guid TopicId { get; set; }
    public DateTime LessonDate { get; set; }
    public bool IsCollected { get; set; }

    public Topic Topic { get; set; } = null!;
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();

    public static DailyLesson Create(
        string userId,
        Guid topicId,
        DateTime lessonDate)
    {
        return new DailyLesson
        {
            UserId = userId,
            TopicId = topicId,
            LessonDate = lessonDate.Date,
            IsCollected = false
        };
    }

    public void Collect()
    {
        IsCollected = true;
    }
}
