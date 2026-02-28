namespace Learn.Application.Profile.GetProfile.Models;

public record UserProfileVm
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public DateTime JoinedDate { get; init; }
    public int TotalXP { get; init; }
    public int Level { get; init; }
    public string LevelTitle { get; init; } = string.Empty;
    public int CurrentStreak { get; init; }
    public int LongestStreak { get; init; }
}
