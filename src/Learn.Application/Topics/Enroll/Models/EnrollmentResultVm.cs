namespace Learn.Application.Topics.Enroll.Models;

public record EnrollmentResultVm
{
    public Guid EnrollmentId { get; init; }
    public Guid TopicId { get; init; }
    public string TopicName { get; init; } = string.Empty;
    public int CurrentUnitIndex { get; init; }
    public int CurrentLessonIndex { get; init; }
}
