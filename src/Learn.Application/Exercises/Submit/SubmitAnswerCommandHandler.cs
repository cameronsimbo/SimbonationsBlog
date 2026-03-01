using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using Learn.Application.Exercises.Submit.Models;
using Learn.Domain.Entities;
using Learn.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Exercises.Submit;

public class SubmitAnswerCommandHandler : IRequestHandler<SubmitAnswerCommand, ExerciseAttemptResultVm>
{
    private readonly ILearnDbContext _db;
    private readonly IAIEvaluationService _aiService;
    private readonly ICurrentUser _currentUser;
    private readonly int _dailyLimit;

    public SubmitAnswerCommandHandler(
        ILearnDbContext db,
        IAIEvaluationService aiService,
        ICurrentUser currentUser,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _db = db;
        _aiService = aiService;
        _currentUser = currentUser;
        _dailyLimit = int.TryParse(configuration["DailySubmissionLimit"], out int limit) ? limit : 50;
    }

    public async Task<ExerciseAttemptResultVm> Handle(SubmitAnswerCommand command, CancellationToken cancellationToken)
    {
        string userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User must be authenticated.");

        // Check daily limit
        int todaySubmissions = await _db.ExerciseAttempts
            .CountAsync(a => a.UserId == userId
                && a.CreatedDate.Date == DateTime.UtcNow.Date, cancellationToken);

        if (todaySubmissions >= _dailyLimit)
        {
            DateTime resetTime = DateTime.UtcNow.Date.AddDays(1);
            throw new DailyLimitExceededException(resetTime);
        }

        // Load exercise with lesson and unit context
        Exercise? exercise = await _db.Exercises
            .Include(e => e.Lesson)
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Topic)
            .FirstOrDefaultAsync(e => e.Id == command.ExerciseId, cancellationToken);

        if (exercise is null)
        {
            throw new NotFoundException(nameof(Exercise), command.ExerciseId);
        }

        // Get attempt number
        int attemptNumber = await _db.ExerciseAttempts
            .CountAsync(a => a.ExerciseId == command.ExerciseId && a.UserId == userId, cancellationToken) + 1;

        // Call AI evaluation
        EvaluationRequest evalRequest = new()
        {
            UserAnswer = command.UserAnswer,
            ExerciseType = exercise.ExerciseType,
            Prompt = exercise.Prompt,
            ReferenceAnswer = exercise.ReferenceAnswer,
            SubjectDomain = exercise.Lesson.Unit.Topic.SubjectDomain,
            DifficultyLevel = exercise.DifficultyLevel
        };

        AIEvaluationResult aiResult = await _aiService.EvaluateAnswerAsync(evalRequest, cancellationToken);

        // Get user streak for XP calculation
        UserStreak? streak = await _db.UserStreaks
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        int currentStreak = streak?.CurrentStreak ?? 0;
        int xpEarned = XPCalculator.Calculate(aiResult.Score, currentStreak);

        // Create attempt
        ExerciseAttempt attempt = ExerciseAttempt.Create(
            command.ExerciseId,
            userId,
            command.UserAnswer,
            aiResult.Score,
            aiResult.IsPassing,
            aiResult.Feedback,
            attemptNumber,
            command.TimeTakenSeconds,
            isAudioSubmission: false,
            suggestedCorrection: aiResult.SuggestedCorrection,
            detailedBreakdown: aiResult.DetailedBreakdown,
            isReview: command.IsReview);

        attempt.XPEarned = xpEarned;
        _db.ExerciseAttempts.Add(attempt);

        // Update streak
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

        leaderboardEntry.AddXP(xpEarned);

        await _db.SaveChangesAsync(cancellationToken);

        // Check if lesson is complete
        int totalExercises = await _db.Exercises
            .CountAsync(e => e.LessonId == exercise.LessonId, cancellationToken);

        int passedExercises = await _db.ExerciseAttempts
            .Where(a => a.UserId == userId && a.Exercise.LessonId == exercise.LessonId && a.IsPassing == true)
            .Select(a => a.ExerciseId)
            .Distinct()
            .CountAsync(cancellationToken);

        bool isLessonComplete = passedExercises >= totalExercises;

        int dailySubmissionsRemaining = _dailyLimit - todaySubmissions - 1;

        return new ExerciseAttemptResultVm
        {
            AttemptId = attempt.Id,
            Score = aiResult.Score,
            IsPassing = aiResult.IsPassing,
            Feedback = aiResult.Feedback,
            SuggestedCorrection = aiResult.SuggestedCorrection,
            DetailedBreakdown = aiResult.DetailedBreakdown,
            XPEarned = xpEarned,
            DailySubmissionsRemaining = Math.Max(0, dailySubmissionsRemaining),
            IsLessonComplete = isLessonComplete,
            GradedBy = aiResult.GradedBy
        };
    }
}
