using Blog.Application.Articles.Create;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using FluentValidation.TestHelper;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Blog.Application.Tests.Articles;

public class CreateArticleValidatorTests
{
    private readonly CreateArticleValidator _validator;

    public CreateArticleValidatorTests()
    {
        Mock<IBlogDbContext> mockDb = new();
        Author testAuthor = new() { DisplayName = "Test", Email = "test@test.com" };
        Category testCategory = Category.Create("Test", "Test category");
        _testAuthorId = testAuthor.Id;
        _testCategoryId = testCategory.Id;
        List<Author> authors = new() { testAuthor };
        List<Category> categories = new() { testCategory };
        mockDb.Setup(d => d.Authors).Returns(authors.AsQueryable().BuildMockDbSet().Object);
        mockDb.Setup(d => d.Categories).Returns(categories.AsQueryable().BuildMockDbSet().Object);
        _validator = new CreateArticleValidator(mockDb.Object);
    }

    private readonly Guid _testAuthorId;
    private readonly Guid _testCategoryId;

    [Fact]
    public async Task Should_HaveError_WhenTitleIsEmpty()
    {
        CreateArticleCommand command = new() { Title = string.Empty };
        TestValidationResult<CreateArticleCommand> result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Title);
    }

    [Fact]
    public async Task Should_HaveError_WhenTitleExceedsMaxLength()
    {
        CreateArticleCommand command = new() { Title = new string('a', 201) };
        TestValidationResult<CreateArticleCommand> result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Title);
    }

    [Fact]
    public async Task Should_HaveError_WhenContentIsEmpty()
    {
        CreateArticleCommand command = new() { Content = string.Empty };
        TestValidationResult<CreateArticleCommand> result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Content);
    }

    [Fact]
    public async Task Should_HaveError_WhenAuthorIdIsEmpty()
    {
        CreateArticleCommand command = new() { AuthorId = Guid.Empty };
        TestValidationResult<CreateArticleCommand> result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.AuthorId);
    }

    [Fact]
    public async Task Should_HaveError_WhenCategoryIdIsEmpty()
    {
        CreateArticleCommand command = new() { CategoryId = Guid.Empty };
        TestValidationResult<CreateArticleCommand> result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public async Task Should_NotHaveErrors_WhenCommandIsValid()
    {
        CreateArticleCommand command = new()
        {
            Title = "Valid Title",
            Content = "Valid content",
            Excerpt = "Valid excerpt",
            AuthorId = _testAuthorId,
            CategoryId = _testCategoryId
        };

        TestValidationResult<CreateArticleCommand> result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
