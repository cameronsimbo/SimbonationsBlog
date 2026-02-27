using FluentValidation;

namespace Blog.Application.Categories.Upsert;

public class UpsertCategoryValidator : AbstractValidator<UpsertCategoryCommand>
{
    public UpsertCategoryValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.Description)
            .MaximumLength(500);
    }
}
