using Learn.Domain.Enums;

namespace Learn.Domain.Entities;

public class Achievement : CreatedEntity<Achievement>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public AchievementType AchievementType { get; set; }
    public int Threshold { get; set; }

    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

    public static Achievement Create(
        string name,
        string description,
        AchievementType achievementType,
        int threshold,
        string? iconUrl = null)
    {
        return new Achievement
        {
            Name = name,
            Description = description,
            AchievementType = achievementType,
            Threshold = threshold,
            IconUrl = iconUrl
        };
    }
}
