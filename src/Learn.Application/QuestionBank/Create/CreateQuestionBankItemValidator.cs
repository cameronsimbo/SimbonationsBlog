using FluentValidation;

namespace Learn.Application.QuestionBank.Create;

public class CreateQuestionBankItemValidator : AbstractValidator<CreateQuestionBankItemCommand>
{
    public CreateQuestionBankItemValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty().WithMessage("Prompt is required.")
            .MaximumLength(2000).WithMessage("Prompt must not exceed 2000 characters.");

        RuleFor(x => x.ReferenceAnswer)
            .NotEmpty().WithMessage("Reference answer is required.")
            .MaximumLength(5000).WithMessage("Reference answer must not exceed 5000 characters.");

        RuleFor(x => x.SubjectDomain)
            .IsInEnum().WithMessage("Invalid subject domain.");

        RuleFor(x => x.ExerciseType)
            .IsInEnum().WithMessage("Invalid exercise type.");

        RuleFor(x => x.DifficultyLevel)
            .IsInEnum().WithMessage("Invalid difficulty level.");
    }
}
