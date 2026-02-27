namespace Learn.Domain.Entities;

public class UserAchievement : BaseEntity<UserAchievement>
{
    public string UserId { get; set; } = string.Empty;
    public Guid AchievementId { get; set; }
    public DateTime EarnedDate { get; set; } = DateTime.UtcNow;

    public Achievement Achievement { get; set; } = null!;
}
