using Blog.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Blog.Domain.Tests.Entities;

public class CategoryTests
{
    [Fact]
    public void Create_ShouldSetNameAndGenerateSlug()
    {
        // Arrange & Act
        Category category = Category.Create("Technology", "Tech articles");

        // Assert
        category.Name.Should().Be("Technology");
        category.Description.Should().Be("Tech articles");
        category.Slug.Should().Be("technology");
    }
}
