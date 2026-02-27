using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using Learn.Domain.Entities;
using Learn.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainUnit = Learn.Domain.Entities.Unit;

namespace Learn.Application.Sessions.CompleteSession;

public class CompleteSessionCommandHandler : IRequestHandler<CompleteSessionCommand, SessionCompleteVm>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CompleteSessionCommandHandler(ILearnDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<SessionCompleteVm> Handle(CompleteSessionCommand command, CancellationToken cancellationToken)
    {
        string userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User must be authenticated.");

        UserTopicEnrollment? enrollment = await _db.UserTopicEnrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.TopicId == command.TopicId && e.IsActive == true, cancellationToken);

        if (enrollment is null)
        {
            throw new NotFoundException("Enrollment", $"User is not enrolled in topic {command.TopicId}");
        }

        int totalScore = 0;
        int totalXP = 0;
        int reviewItemsCreated = 0;

        foreach (ExerciseResultDto result in command.Results)
        {
            totalScore += result.Score;
            totalXP += result.XPEarned;

            // Create or update review items for failed exercises
            if (result.IsPassing == false && result.IsReview == false)
            {
                ReviewItem? existingReview = await _db.ReviewItems
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ExerciseId == result.ExerciseId, cancellationToken);

                if (existingReview is null)
                {
                    ReviewItem newReview = ReviewItem.Create(userId, result.ExerciseId);
                    _db.ReviewItems.Add(newReview);
                    reviewItemsCreated++;
                }
            }

            // Update existing review items
            if (result.IsReview)
            {
                ReviewItem? reviewItem = await _db.ReviewItems
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ExerciseId == result.ExerciseId, cancellationToken);

                if (reviewItem is not null)
                {
                    reviewItem.RecordReview(result.Score);
                }
            }
        }

        // Calculate session bonus
        int sessionBonus = SessionEngine.CalculateSessionXPBonus(totalScore, command.Results.Count);
        totalXP += sessionBonus;

        // Update enrollment
        enrollment.RecordSession(totalXP);

        // Check if lesson is complete and advance
        List<DomainUnit> units = await _db.Units
            .Where(u => u.TopicId == command.TopicId)
            .OrderBy(u => u.OrderIndex)
            .ToListAsync(cancellationToken);

        int unitIndex = Math.Min(enrollment.CurrentUnitIndex, units.Count - 1);
        DomainUnit currentUnit = units[unitIndex];

        List<Lesson> lessons = await _db.Lessons
            .Where(l => l.UnitId == currentUnit.Id)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync(cancellationToken);

        int lessonIndex = Math.Min(enrollment.CurrentLessonIndex, lessons.Count - 1);
        Lesson currentLesson = lessons[lessonIndex];

        // Check lesson completion
        int totalExercises = await _db.Exercises.CountAsync(e => e.LessonId == currentLesson.Id, cancellationToken);
        int passedExercises = await _db.ExerciseAttempts
            .Where(a => a.UserId == userId && a.Exercise.LessonId == currentLesson.Id && a.IsPassing == true)
            .Select(a => a.ExerciseId)
            .Distinct()
            .CountAsync(cancellationToken);

        bool lessonComplete = passedExercises >= totalExercises && totalExercises > 0;
        bool unitAdvanced = false;

        if (lessonComplete)
        {
            int oldUnitIndex = enrollment.CurrentUnitIndex;
            enrollment.AdvanceToNextLesson(lessons.Count, units.Count);
            unitAdvanced = enrollment.CurrentUnitIndex > oldUnitIndex;

            // Update UserProgress
            UserProgress? progress = await _db.UserProgress
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == currentLesson.Id, cancellationToken);

            if (progress is null)
            {
                progress = UserProgress.Create(userId, command.TopicId, currentLesson.Id, totalExercises);
                _db.UserProgress.Add(progress);
            }

            progress.RecordAttempt(totalScore / Math.Max(command.Results.Count, 1));
        }

        // Update streak
        UserStreak? streak = await _db.UserStreaks
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (streak is not null)
        {
            streak.RecordActivity(DateTime.UtcNow);
        }

        // Update leaderboard
        DateTime weekStart = LeaderboardEntry.GetCurrentWeekStart(DateTime.UtcNow);
        LeaderboardEntry? leaderboardEntry = await _db.LeaderboardEntries
            .FirstOrDefaultAsync(l => l.UserId == userId && l.WeekStartDate == weekStart, cancellationToken);

        if (leaderboardEntry is null)
        {
            leaderboardEntry = LeaderboardEntry.Create(userId, weekStart);
            _db.LeaderboardEntries.Add(leaderboardEntry);
        }

        leaderboardEntry.AddXP(totalXP);

        await _db.SaveChangesAsync(cancellationToken);

        // Calculate level info
        int allTimeXP = await _db.UserTopicEnrollments
            .Where(e => e.UserId == userId)
            .SumAsync(e => e.TotalXPEarned, cancellationToken);

        int newLevel = LevelThresholds.GetLevel(allTimeXP);
        string levelTitle = LevelThresholds.GetTitle(allTimeXP);
        double levelProgress = LevelThresholds.GetLevelProgress(allTimeXP);

        int averageScore = command.Results.Count > 0 ? totalScore / command.Results.Count : 0;

        return new SessionCompleteVm
        {
            TotalXPEarned = totalXP,
            SessionBonus = sessionBonus,
            ExercisesCompleted = command.Results.Count,
            AverageScore = averageScore,
            LessonComplete = lessonComplete,
            UnitAdvanced = unitAdvanced,
            NewLevel = newLevel,
            LevelTitle = levelTitle,
            LevelProgress = levelProgress,
            CurrentStreak = streak?.CurrentStreak ?? 0,
            ReviewItemsCreated = reviewItemsCreated
        };
    }
}
