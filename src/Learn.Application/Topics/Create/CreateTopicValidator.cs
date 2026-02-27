using FluentValidation;
using Learn.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Topics.Create;

public class CreateTopicValidator : AbstractValidator<CreateTopicCommand>
{
    private readonly ILearnDbContext _db;

    public CreateTopicValidator(ILearnDbContext db)
    {
        _db = db;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
            .MustAsync(BeUniqueName).WithMessage("A topic with this name already exists.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.SubjectDomain)
            .IsInEnum().WithMessage("Invalid subject domain.");

        RuleFor(x => x.DifficultyLevel)
            .IsInEnum().WithMessage("Invalid difficulty level.");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken ct)
    {
        return await _db.Topics.AllAsync(t => t.Name != name, ct);
    }
}
