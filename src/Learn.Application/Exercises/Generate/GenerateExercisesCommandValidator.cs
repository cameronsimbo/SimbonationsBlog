using FluentValidation;

namespace Learn.Application.Exercises.Generate;

public class GenerateExercisesCommandValidator : AbstractValidator<GenerateExercisesCommand>
{
    public GenerateExercisesCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty()
            .WithMessage("LessonId is required.");

        RuleFor(x => x.Count)
            .InclusiveBetween(1, 20)
            .WithMessage("Count must be between 1 and 20.");
    }
}
