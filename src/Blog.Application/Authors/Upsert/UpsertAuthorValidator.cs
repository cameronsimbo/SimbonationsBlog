using FluentValidation;

namespace Blog.Application.Authors.Upsert;

public class UpsertAuthorValidator : AbstractValidator<UpsertAuthorCommand>
{
    public UpsertAuthorValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.");
    }
}
