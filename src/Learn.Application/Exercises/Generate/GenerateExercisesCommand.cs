using Learn.Application.Exercises.GetByLesson;
using MediatR;

namespace Learn.Application.Exercises.Generate;

public record GenerateExercisesCommand : IRequest<List<ExerciseVm>>
{
    public Guid LessonId { get; init; }
    public int Count { get; init; } = 5;
}
