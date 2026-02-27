using Blog.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Articles.Create;

public class CreateArticleValidator : AbstractValidator<CreateArticleCommand>
{
    private readonly IBlogDbContext _db;

    public CreateArticleValidator(IBlogDbContext db)
    {
        _db = db;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(x => x.AuthorId)
            .MustAsync(AuthorExists).WithMessage("Author does not exist.");

        RuleFor(x => x.CategoryId)
            .MustAsync(CategoryExists).WithMessage("Category does not exist.");
    }

    private async Task<bool> AuthorExists(Guid authorId, CancellationToken ct)
    {
        return await _db.Authors.AnyAsync(a => a.Id == authorId, ct);
    }

    private async Task<bool> CategoryExists(Guid categoryId, CancellationToken ct)
    {
        return await _db.Categories.AnyAsync(c => c.Id == categoryId, ct);
    }
}
