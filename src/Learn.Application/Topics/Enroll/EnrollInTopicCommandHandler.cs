using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using Learn.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Topics.Enroll;

public class EnrollInTopicCommandHandler : IRequestHandler<EnrollInTopicCommand, EnrollmentResultVm>
{
    private readonly ILearnDbContext _db;
    private readonly ICurrentUser _currentUser;

    public EnrollInTopicCommandHandler(ILearnDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EnrollmentResultVm> Handle(EnrollInTopicCommand command, CancellationToken cancellationToken)
    {
        string userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User must be authenticated.");

        Topic? topic = await _db.Topics
            .FirstOrDefaultAsync(t => t.Id == command.TopicId, cancellationToken);

        if (topic is null)
        {
            throw new NotFoundException(nameof(Topic), command.TopicId);
        }

        UserTopicEnrollment? existing = await _db.UserTopicEnrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.TopicId == command.TopicId, cancellationToken);

        if (existing is not null)
        {
            existing.IsActive = true;

            await _db.SaveChangesAsync(cancellationToken);

            return new EnrollmentResultVm
            {
                EnrollmentId = existing.Id,
                TopicId = topic.Id,
                TopicName = topic.Name,
                CurrentUnitIndex = existing.CurrentUnitIndex,
                CurrentLessonIndex = existing.CurrentLessonIndex
            };
        }

        UserTopicEnrollment enrollment = UserTopicEnrollment.Create(userId, command.TopicId);
        _db.UserTopicEnrollments.Add(enrollment);

        await _db.SaveChangesAsync(cancellationToken);

        return new EnrollmentResultVm
        {
            EnrollmentId = enrollment.Id,
            TopicId = topic.Id,
            TopicName = topic.Name,
            CurrentUnitIndex = 0,
            CurrentLessonIndex = 0
        };
    }
}
