using Learn.Application.Common.Interfaces;
using Learn.Application.Profile.GetProfile.Models;
using Learn.Domain.Entities;
using Learn.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Profile.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfileVm>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetProfileQueryHandler(ILearnDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<UserProfileVm> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        string userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        string? email = _currentUser.Email;
        string? displayName = _currentUser.DisplayName;

        // Get streak for join date
        UserStreak? streak = await _db.UserStreaks.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        DateTime joinedDate = streak?.CreatedDate ?? DateTime.UtcNow;
        int currentStreak = streak?.CurrentStreak ?? 0;
        int longestStreak = streak?.LongestStreak ?? 0;

        // Get all enrollments for XP/level
        var enrollments = await _db.UserTopicEnrollments
            .Where(e => e.UserId == userId && e.IsActive)
            .ToListAsync(cancellationToken);
        int totalXP = enrollments.Sum(e => e.TotalXPEarned);
        int level = LevelThresholds.GetLevel(totalXP);
        string levelTitle = LevelThresholds.GetTitle(totalXP);

        return new UserProfileVm
        {
            UserId = userId,
            Email = email ?? string.Empty,
            DisplayName = displayName,
            JoinedDate = joinedDate,
            TotalXP = totalXP,
            Level = level,
            LevelTitle = levelTitle,
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak
        };
    }
}
