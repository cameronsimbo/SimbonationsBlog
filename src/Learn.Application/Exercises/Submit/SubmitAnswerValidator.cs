using FluentValidation;

namespace Learn.Application.Exercises.Submit;

public class SubmitAnswerValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerValidator()
    {
        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("Exercise ID is required.");

        RuleFor(x => x.UserAnswer)
            .NotEmpty().WithMessage("Answer is required.")
            .MaximumLength(5000).WithMessage("Answer must not exceed 5000 characters.");

        RuleFor(x => x.TimeTakenSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Time taken must be non-negative.");
    }
}
