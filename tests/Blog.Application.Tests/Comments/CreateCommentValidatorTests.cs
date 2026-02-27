using Blog.Application.Comments.Create;
using FluentValidation.TestHelper;
using Xunit;

namespace Blog.Application.Tests.Comments;

public class CreateCommentValidatorTests
{
    private readonly CreateCommentValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenAuthorNameIsEmpty()
    {
        CreateCommentCommand command = new() { AuthorName = string.Empty };
        TestValidationResult<CreateCommentCommand> result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.AuthorName);
    }

    [Fact]
    public void Should_HaveError_WhenEmailIsInvalid()
    {
        CreateCommentCommand command = new() { AuthorEmail = "not-an-email" };
        TestValidationResult<CreateCommentCommand> result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.AuthorEmail);
    }

    [Fact]
    public void Should_HaveError_WhenContentIsEmpty()
    {
        CreateCommentCommand command = new() { Content = string.Empty };
        TestValidationResult<CreateCommentCommand> result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Content);
    }

    [Fact]
    public void Should_NotHaveErrors_WhenCommandIsValid()
    {
        CreateCommentCommand command = new()
        {
            ArticleId = Guid.NewGuid(),
            AuthorName = "John Doe",
            AuthorEmail = "john@example.com",
            Content = "Great article!"
        };

        TestValidationResult<CreateCommentCommand> result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
