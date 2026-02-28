using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using Learn.Application.Sessions.StartSession.Models;
using Learn.Domain.Entities;
using Learn.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainUnit = Learn.Domain.Entities.Unit;

namespace Learn.Application.Sessions.StartSession;

public class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, SessionVm>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAIEvaluationService _aiService;

    public StartSessionCommandHandler(
        ILearnDbContext db,
        ICurrentUser currentUser,
        IAIEvaluationService aiService)
    {
        _db = db;
        _currentUser = currentUser;
        _aiService = aiService;
    }

    public async Task<SessionVm> Handle(StartSessionCommand command, CancellationToken cancellationToken)
    {
        string userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User must be authenticated.");

        UserTopicEnrollment? enrollment = await _db.UserTopicEnrollments
            .Include(e => e.Topic)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.TopicId == command.TopicId && e.IsActive == true, cancellationToken);

        if (enrollment is null)
        {
            throw new NotFoundException("Enrollment", $"User is not enrolled in topic {command.TopicId}");
        }

        List<DomainUnit> units = await _db.Units
            .Where(u => u.TopicId == command.TopicId)
            .OrderBy(u => u.OrderIndex)
            .ToListAsync(cancellationToken);

        if (units.Count == 0)
        {
            throw new NotFoundException("Units", $"No units found for topic {command.TopicId}");
        }

        int unitIndex = Math.Min(enrollment.CurrentUnitIndex, units.Count - 1);
        DomainUnit currentUnit = units[unitIndex];

        List<Lesson> lessons = await _db.Lessons
            .Where(l => l.UnitId == currentUnit.Id)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync(cancellationToken);

        if (lessons.Count == 0)
        {
            throw new NotFoundException("Lessons", $"No lessons found for unit {currentUnit.Id}");
        }

        int lessonIndex = Math.Min(enrollment.CurrentLessonIndex, lessons.Count - 1);
        Lesson currentLesson = lessons[lessonIndex];

        // Get review items due
        List<ReviewItem> dueReviews = await _db.ReviewItems
            .Include(r => r.Exercise)
            .Where(r => r.UserId == userId && r.NextReviewDate.Date <= DateTime.UtcNow.Date)
            .OrderBy(r => r.NextReviewDate)
            .Take(SessionEngine.ExercisesPerSession)
            .ToListAsync(cancellationToken);

        (int newCount, int reviewCount) = SessionEngine.CalculateSessionMix(dueReviews.Count);

        // Get existing exercises for the current lesson
        // IsHidden is a computed C# property (not a DB column) — inline the condition
        List<Exercise> existingExercises = await _db.Exercises
            .Where(e => e.LessonId == currentLesson.Id && (e.UpvoteCount - e.DownvoteCount) >= -3)
            .OrderBy(e => e.OrderIndex)
            .ToListAsync(cancellationToken);

        // Generate exercises if we don't have enough
        if (existingExercises.Count < newCount)
        {
            int toGenerate = newCount - existingExercises.Count;
            Topic topic = enrollment.Topic;

            ExerciseGenerationRequest genRequest = new()
            {
                SubjectDomain = topic.SubjectDomain,
                DifficultyLevel = topic.DifficultyLevel,
                ExerciseType = Domain.Enums.ExerciseType.FreeTextResponse,
                Count = toGenerate,
                TopicName = topic.Name,
                UnitName = currentUnit.Name,
                LessonName = currentLesson.Name,
                KeyConcepts = topic.KeyConcepts,
                GenerationGuidance = topic.GenerationGuidance,
                LessonContext = currentLesson.GenerationContext
            };

            List<GeneratedExercise> generated = await _aiService.GenerateExercisesAsync(genRequest, cancellationToken);

            int orderStart = existingExercises.Count;
            foreach (GeneratedExercise gen in generated)
            {
                Exercise exercise = Exercise.Create(
                    currentLesson.Id,
                    orderStart++,
                    gen.ExerciseType,
                    topic.DifficultyLevel,
                    gen.Prompt,
                    gen.ReferenceAnswer,
                    context: gen.Context,
                    hints: gen.Hints);

                _db.Exercises.Add(exercise);
                existingExercises.Add(exercise);
            }

            currentLesson.HasGeneratedExercises = true;
            currentLesson.ExerciseCount = existingExercises.Count;
            await _db.SaveChangesAsync(cancellationToken);
        }

        // Build session exercise list
        List<SessionExerciseVm> sessionExercises = new();

        // Add new exercises (pick ones user hasn't passed yet, or any if all passed)
        List<Guid> passedExerciseIds = await _db.ExerciseAttempts
            .Where(a => a.UserId == userId && a.Exercise.LessonId == currentLesson.Id && a.IsPassing == true)
            .Select(a => a.ExerciseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Exercise> unpassedExercises = existingExercises
            .Where(e => passedExerciseIds.Contains(e.Id) == false)
            .ToList();

        List<Exercise> newExercisePool = unpassedExercises.Count > 0 ? unpassedExercises : existingExercises;
        List<Exercise> selectedNew = newExercisePool.Take(newCount).ToList();

        foreach (Exercise exercise in selectedNew)
        {
            sessionExercises.Add(new SessionExerciseVm
            {
                ExerciseId = exercise.Id,
                Prompt = exercise.Prompt,
                Context = exercise.Context,
                Hints = exercise.Hints,
                ExerciseType = (int)exercise.ExerciseType,
                DifficultyLevel = (int)exercise.DifficultyLevel,
                IsReview = false
            });
        }

        // Add review exercises
        List<ReviewItem> selectedReviews = dueReviews.Take(reviewCount).ToList();
        foreach (ReviewItem review in selectedReviews)
        {
            sessionExercises.Add(new SessionExerciseVm
            {
                ExerciseId = review.Exercise.Id,
                Prompt = review.Exercise.Prompt,
                Context = review.Exercise.Context,
                Hints = review.Exercise.Hints,
                ExerciseType = (int)review.Exercise.ExerciseType,
                DifficultyLevel = (int)review.Exercise.DifficultyLevel,
                IsReview = true
            });
        }

        return new SessionVm
        {
            EnrollmentId = enrollment.Id,
            TopicId = enrollment.TopicId,
            TopicName = enrollment.Topic.Name,
            CurrentLessonId = currentLesson.Id,
            CurrentLessonName = currentLesson.Name,
            CurrentUnitName = currentUnit.Name,
            UnitIndex = unitIndex,
            LessonIndex = lessonIndex,
            Exercises = sessionExercises,
            NewExerciseCount = selectedNew.Count,
            ReviewExerciseCount = selectedReviews.Count
        };
    }
}
