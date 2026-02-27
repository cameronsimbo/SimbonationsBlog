using FluentValidation;

namespace Blog.Application.Comments.Create;

public class CreateCommentValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentValidator()
    {
        RuleFor(c => c.ArticleId)
            .NotEmpty();

        RuleFor(c => c.AuthorName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.AuthorEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(c => c.Content)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
