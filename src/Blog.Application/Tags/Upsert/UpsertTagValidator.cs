using FluentValidation;

namespace Blog.Application.Tags.Upsert;

public class UpsertTagValidator : AbstractValidator<UpsertTagCommand>
{
    public UpsertTagValidator()
    {
        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(50);
    }
}
