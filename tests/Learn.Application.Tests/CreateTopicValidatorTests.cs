using FluentAssertions;
using Learn.Application.Common.Interfaces;
using Learn.Application.Topics.Create;
using Learn.Domain.Entities;
using Learn.Domain.Enums;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Learn.Application.Tests;

public class CreateTopicValidatorTests
{
    private readonly CreateTopicValidator _validator;
    private readonly Mock<ILearnDbContext> _dbMock;

    public CreateTopicValidatorTests()
    {
        _dbMock = new Mock<ILearnDbContext>();
        List<Topic> emptyTopics = new();
        _dbMock.Setup(x => x.Topics).Returns(emptyTopics.AsQueryable().BuildMockDbSet().Object);
        _validator = new CreateTopicValidator(_dbMock.Object);
    }

    [Fact]
    public async Task Validate_ValidCommand_Passes()
    {
        CreateTopicCommand command = new()
        {
            Name = "History 101",
            Description = "Learn world history",
            SubjectDomain = SubjectDomain.History,
            DifficultyLevel = DifficultyLevel.Beginner
        };

        FluentValidation.Results.ValidationResult result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyName_Fails()
    {
        CreateTopicCommand command = new()
        {
            Name = "",
            Description = "Description",
            SubjectDomain = SubjectDomain.History,
            DifficultyLevel = DifficultyLevel.Beginner
        };

        FluentValidation.Results.ValidationResult result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_EmptyDescription_Fails()
    {
        CreateTopicCommand command = new()
        {
            Name = "History 101",
            Description = "",
            SubjectDomain = SubjectDomain.History,
            DifficultyLevel = DifficultyLevel.Beginner
        };

        FluentValidation.Results.ValidationResult result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
