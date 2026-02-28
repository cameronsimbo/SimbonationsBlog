using Learn.Application.Exercises.GetByLesson.Models;
using Learn.Domain.Enums;
using MediatR;

namespace Learn.Application.Exercises.GetByLesson;

public record GetExercisesByLessonQuery : IRequest<List<ExerciseVm>>
{
    public Guid LessonId { get; init; }
}
