using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using Learn.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainUnit = Learn.Domain.Entities.Unit;

namespace Learn.Application.Progress.GetLearningPath;

public class GetLearningPathQueryHandler : IRequestHandler<GetLearningPathQuery, LearningPathVm>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetLearningPathQueryHandler(ILearnDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<LearningPathVm> Handle(GetLearningPathQuery request, CancellationToken cancellationToken)
    {
        string userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User must be authenticated.");

        UserTopicEnrollment? enrollment = await _db.UserTopicEnrollments
            .Include(e => e.Topic)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.TopicId == request.TopicId, cancellationToken);

        if (enrollment is null)
        {
            throw new NotFoundException("Enrollment", $"User is not enrolled in topic {request.TopicId}");
        }

        List<DomainUnit> units = await _db.Units
            .Include(u => u.Lessons)
            .Where(u => u.TopicId == request.TopicId)
            .OrderBy(u => u.OrderIndex)
            .ToListAsync(cancellationToken);

        // Get all user progress for this topic
        List<UserProgress> progressList = await _db.UserProgress
            .Where(p => p.UserId == userId && p.TopicId == request.TopicId)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, UserProgress> progressByLesson = progressList.ToDictionary(p => p.LessonId);

        List<PathUnitVm> pathUnits = new();

        for (int unitIdx = 0; unitIdx < units.Count; unitIdx++)
        {
            DomainUnit unit = units[unitIdx];
            List<Lesson> orderedLessons = unit.Lessons.OrderBy(l => l.OrderIndex).ToList();

            bool isUnitCurrent = unitIdx == enrollment.CurrentUnitIndex;
            bool isUnitCompleted = unitIdx < enrollment.CurrentUnitIndex;
            bool isUnitLocked = unitIdx > enrollment.CurrentUnitIndex;

            List<PathLessonVm> pathLessons = new();

            for (int lessonIdx = 0; lessonIdx < orderedLessons.Count; lessonIdx++)
            {
                Lesson lesson = orderedLessons[lessonIdx];
                progressByLesson.TryGetValue(lesson.Id, out UserProgress? progress);

                bool isLessonCurrent = isUnitCurrent && lessonIdx == enrollment.CurrentLessonIndex;
                bool isLessonCompleted = isUnitCompleted
                    || (isUnitCurrent && lessonIdx < enrollment.CurrentLessonIndex)
                    || (progress is not null && progress.IsCompleted);
                bool isLessonLocked = isUnitLocked
                    || (isUnitCurrent && lessonIdx > enrollment.CurrentLessonIndex);

                pathLessons.Add(new PathLessonVm
                {
                    LessonId = lesson.Id,
                    Name = lesson.Name,
                    OrderIndex = lesson.OrderIndex,
                    IsCurrent = isLessonCurrent,
                    IsCompleted = isLessonCompleted,
                    IsLocked = isLessonLocked,
                    Crowns = progress?.MasteryLevel ?? 0,
                    BestScore = progress?.BestScore ?? 0
                });
            }

            pathUnits.Add(new PathUnitVm
            {
                UnitId = unit.Id,
                Name = unit.Name,
                Description = unit.Description,
                OrderIndex = unit.OrderIndex,
                IsCurrent = isUnitCurrent,
                IsCompleted = isUnitCompleted,
                IsLocked = isUnitLocked,
                Lessons = pathLessons
            });
        }

        return new LearningPathVm
        {
            TopicId = enrollment.TopicId,
            TopicName = enrollment.Topic.Name,
            CurrentUnitIndex = enrollment.CurrentUnitIndex,
            CurrentLessonIndex = enrollment.CurrentLessonIndex,
            TotalXPEarned = enrollment.TotalXPEarned,
            Units = pathUnits
        };
    }
}
