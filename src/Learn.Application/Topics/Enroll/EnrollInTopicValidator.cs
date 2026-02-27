using FluentValidation;

namespace Learn.Application.Topics.Enroll;

public class EnrollInTopicValidator : AbstractValidator<EnrollInTopicCommand>
{
    public EnrollInTopicValidator()
    {
        RuleFor(x => x.TopicId)
            .NotEmpty().WithMessage("Topic ID is required.");
    }
}
