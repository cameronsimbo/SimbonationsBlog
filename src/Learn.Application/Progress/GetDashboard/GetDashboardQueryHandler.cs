using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using Learn.Application.Progress.GetDashboard.Models;
using Learn.Domain.Entities;
using Learn.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainUnit = Learn.Domain.Entities.Unit;

namespace Learn.Application.Progress.GetDashboard;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardVm>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly int _dailyLimit;

    public GetDashboardQueryHandler(
        ILearnDbContext db,
        ICurrentUser currentUser,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _db = db;
        _currentUser = currentUser;
        _dailyLimit = int.TryParse(configuration["DailySubmissionLimit"], out int limit) ? limit : 50;
    }

    public async Task<DashboardVm> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        string userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User must be authenticated.");

        // Get enrollments with topic info
        List<UserTopicEnrollment> enrollments = await _db.UserTopicEnrollments
            .Include(e => e.Topic)
            .Where(e => e.UserId == userId && e.IsActive == true)
            .OrderByDescending(e => e.LastSessionDate)
            .ToListAsync(cancellationToken);

        // Get streak
        UserStreak? streak = await _db.UserStreaks
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        // Calculate total XP
        int totalXP = enrollments.Sum(e => e.TotalXPEarned);

        // Get review items due
        int reviewItemsDue = await _db.ReviewItems
            .CountAsync(r => r.UserId == userId && r.NextReviewDate.Date <= DateTime.UtcNow.Date, cancellationToken);

        // Get daily submissions remaining
        int todaySubmissions = await _db.ExerciseAttempts
            .CountAsync(a => a.UserId == userId && a.CreatedDate.Date == DateTime.UtcNow.Date, cancellationToken);

        // Build enrolled topic VMs with current unit/lesson names
        List<EnrolledTopicVm> enrolledTopics = new();

        foreach (UserTopicEnrollment enrollment in enrollments)
        {
            string currentUnitName = string.Empty;
            string currentLessonName = string.Empty;

            DomainUnit? currentUnit = await _db.Units
                .Where(u => u.TopicId == enrollment.TopicId)
                .OrderBy(u => u.OrderIndex)
                .Skip(enrollment.CurrentUnitIndex)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentUnit is not null)
            {
                currentUnitName = currentUnit.Name;

                Lesson? currentLesson = await _db.Lessons
                    .Where(l => l.UnitId == currentUnit.Id)
                    .OrderBy(l => l.OrderIndex)
                    .Skip(enrollment.CurrentLessonIndex)
                    .FirstOrDefaultAsync(cancellationToken);

                if (currentLesson is not null)
                {
                    currentLessonName = currentLesson.Name;
                }
            }

            enrolledTopics.Add(new EnrolledTopicVm
            {
                TopicId = enrollment.TopicId,
                TopicName = enrollment.Topic.Name,
                TopicDescription = enrollment.Topic.Description,
                SubjectDomain = (int)enrollment.Topic.SubjectDomain,
                DifficultyLevel = (int)enrollment.Topic.DifficultyLevel,
                IconUrl = enrollment.Topic.IconUrl,
                CurrentUnitIndex = enrollment.CurrentUnitIndex,
                CurrentLessonIndex = enrollment.CurrentLessonIndex,
                CurrentUnitName = currentUnitName,
                CurrentLessonName = currentLessonName,
                TotalUnits = enrollment.Topic.TotalUnits,
                TotalXPEarned = enrollment.TotalXPEarned,
                SessionsCompleted = enrollment.SessionsCompleted,
                LastSessionDate = enrollment.LastSessionDate
            });
        }

        int level = LevelThresholds.GetLevel(totalXP);
        string levelTitle = LevelThresholds.GetTitle(totalXP);
        double levelProgress = LevelThresholds.GetLevelProgress(totalXP);
        int xpForNextLevel = LevelThresholds.GetXPForNextLevel(totalXP);

        return new DashboardVm
        {
            TotalXP = totalXP,
            Level = level,
            LevelTitle = levelTitle,
            LevelProgress = levelProgress,
            XPForNextLevel = xpForNextLevel,
            CurrentStreak = streak?.CurrentStreak ?? 0,
            LongestStreak = streak?.LongestStreak ?? 0,
            StreakFreezeCount = streak?.StreakFreezeCount ?? 0,
            EnrolledTopics = enrolledTopics,
            ReviewItemsDue = reviewItemsDue,
            DailySubmissionsRemaining = Math.Max(0, _dailyLimit - todaySubmissions)
        };
    }
}
