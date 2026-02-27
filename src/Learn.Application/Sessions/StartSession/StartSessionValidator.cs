using FluentValidation;

namespace Learn.Application.Sessions.StartSession;

public class StartSessionValidator : AbstractValidator<StartSessionCommand>
{
    public StartSessionValidator()
    {
        RuleFor(x => x.TopicId)
            .NotEmpty().WithMessage("Topic ID is required.");
    }
}
