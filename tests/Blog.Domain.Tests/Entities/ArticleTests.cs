using Blog.Domain.Entities;
using Blog.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Blog.Domain.Tests.Entities;

public class ArticleTests
{
    [Fact]
    public void Create_ShouldSetPropertiesAndGenerateSlug()
    {
        // Arrange & Act
        Article article = Article.Create(
            "My First Blog Post",
            "This is the content.",
            "A short excerpt",
            null,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        article.Title.Should().Be("My First Blog Post");
        article.Slug.Should().Be("my-first-blog-post");
        article.Content.Should().Be("This is the content.");
        article.Excerpt.Should().Be("A short excerpt");
        article.Status.Should().Be(ArticleStatus.Draft);
    }

    [Fact]
    public void Publish_ShouldSetStatusAndPublishedDate()
    {
        // Arrange
        Article article = Article.Create("Test", "Content", "Excerpt", null, Guid.NewGuid(), Guid.NewGuid());

        // Act
        article.Publish();

        // Assert
        article.Status.Should().Be(ArticleStatus.Published);
        article.PublishedDate.Should().NotBeNull();
        article.PublishedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Archive_ShouldSetStatusToArchived()
    {
        // Arrange
        Article article = Article.Create("Test", "Content", "Excerpt", null, Guid.NewGuid(), Guid.NewGuid());

        // Act
        article.Archive();

        // Assert
        article.Status.Should().Be(ArticleStatus.Archived);
    }

    [Fact]
    public void GenerateSlug_ShouldConvertTitleToUrlFriendlySlug()
    {
        // Arrange
        Article article = Article.Create(
            "Hello World! This Is A Test",
            "Content",
            "Excerpt",
            null,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        article.Slug.Should().Be("hello-world-this-is-a-test");
    }
}
