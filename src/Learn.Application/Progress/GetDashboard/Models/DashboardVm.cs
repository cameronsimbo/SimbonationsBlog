namespace Learn.Application.Progress.GetDashboard.Models;

public record DashboardVm
{
    public int TotalXP { get; init; }
    public int Level { get; init; }
    public string LevelTitle { get; init; } = string.Empty;
    public double LevelProgress { get; init; }
    public int XPForNextLevel { get; init; }
    public int CurrentStreak { get; init; }
    public int LongestStreak { get; init; }
    public int StreakFreezeCount { get; init; }
    public List<EnrolledTopicVm> EnrolledTopics { get; init; } = new();
    public int ReviewItemsDue { get; init; }
    public int DailySubmissionsRemaining { get; init; }
}

public record EnrolledTopicVm
{
    public Guid TopicId { get; init; }
    public string TopicName { get; init; } = string.Empty;
    public string TopicDescription { get; init; } = string.Empty;
    public int SubjectDomain { get; init; }
    public int DifficultyLevel { get; init; }
    public string? IconUrl { get; init; }
    public int CurrentUnitIndex { get; init; }
    public int CurrentLessonIndex { get; init; }
    public string CurrentUnitName { get; init; } = string.Empty;
    public string CurrentLessonName { get; init; } = string.Empty;
    public int TotalUnits { get; init; }
    public int TotalXPEarned { get; init; }
    public int SessionsCompleted { get; init; }
    public DateTime? LastSessionDate { get; init; }
}
