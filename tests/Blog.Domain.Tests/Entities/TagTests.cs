using Blog.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Blog.Domain.Tests.Entities;

public class TagTests
{
    [Fact]
    public void Create_ShouldSetNameAndGenerateSlug()
    {
        // Arrange & Act
        Tag tag = Tag.Create("C Sharp");

        // Assert
        tag.Name.Should().Be("C Sharp");
        tag.Slug.Should().Be("c-sharp");
    }
}
