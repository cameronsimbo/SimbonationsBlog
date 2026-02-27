namespace Learn.Domain.Entities;

public class UserTopicEnrollment : CreatedEntity<UserTopicEnrollment>
{
    public string UserId { get; set; } = string.Empty;
    public Guid TopicId { get; set; }
    public int CurrentUnitIndex { get; set; }
    public int CurrentLessonIndex { get; set; }
    public bool IsActive { get; set; } = true;
    public int TotalXPEarned { get; set; }
    public int SessionsCompleted { get; set; }
    public DateTime? LastSessionDate { get; set; }

    public Topic Topic { get; set; } = null!;

    public static UserTopicEnrollment Create(string userId, Guid topicId)
    {
        return new UserTopicEnrollment
        {
            UserId = userId,
            TopicId = topicId,
            CurrentUnitIndex = 0,
            CurrentLessonIndex = 0,
            IsActive = true,
            TotalXPEarned = 0,
            SessionsCompleted = 0
        };
    }

    public void AdvanceToNextLesson(int totalLessonsInUnit, int totalUnitsInTopic)
    {
        CurrentLessonIndex++;

        if (CurrentLessonIndex >= totalLessonsInUnit)
        {
            CurrentLessonIndex = 0;
            CurrentUnitIndex++;

            if (CurrentUnitIndex >= totalUnitsInTopic)
            {
                CurrentUnitIndex = totalUnitsInTopic - 1;
                CurrentLessonIndex = totalLessonsInUnit - 1;
            }
        }
    }

    public void RecordSession(int xpEarned)
    {
        TotalXPEarned += xpEarned;
        SessionsCompleted++;
        LastSessionDate = DateTime.UtcNow;
    }
}
