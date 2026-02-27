using FluentAssertions;
using Learn.Domain.Entities;
using Learn.Domain.Enums;
using Xunit;

namespace Learn.Domain.Tests;

public class TopicTests
{
    [Fact]
    public void Create_ValidParameters_ReturnsTopic()
    {
        Topic topic = Topic.Create("History 101", "Learn world history", SubjectDomain.History, DifficultyLevel.Beginner);

        topic.Name.Should().Be("History 101");
        topic.Description.Should().Be("Learn world history");
        topic.SubjectDomain.Should().Be(SubjectDomain.History);
        topic.DifficultyLevel.Should().Be(DifficultyLevel.Beginner);
        topic.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void Publish_SetsIsPublishedTrue()
    {
        Topic topic = Topic.Create("Test", "Desc", SubjectDomain.Science, DifficultyLevel.Intermediate);

        topic.Publish();

        topic.IsPublished.Should().BeTrue();
    }

    [Fact]
    public void Unpublish_SetsIsPublishedFalse()
    {
        Topic topic = Topic.Create("Test", "Desc", SubjectDomain.Science, DifficultyLevel.Intermediate);
        topic.Publish();

        topic.Unpublish();

        topic.IsPublished.Should().BeFalse();
    }
}
