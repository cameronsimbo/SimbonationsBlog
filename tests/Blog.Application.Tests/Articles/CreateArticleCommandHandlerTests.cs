using Blog.Application.Articles.Create;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Blog.Application.Tests.Articles;

public class CreateArticleCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateArticleAndReturnId()
    {
        // Arrange
        Mock<IBlogDbContext> mockDb = new();
        Mock<DbSet<Article>> mockSet = new();
        mockDb.Setup(d => d.Articles).Returns(mockSet.Object);
        mockDb.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        CreateArticleCommandHandler handler = new(mockDb.Object);
        CreateArticleCommand command = new()
        {
            Title = "Test Article",
            Content = "Test Content",
            Excerpt = "Test Excerpt",
            AuthorId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            TagIds = new List<Guid>()
        };

        // Act
        Guid result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        mockSet.Verify(s => s.Add(It.IsAny<Article>()), Times.Once);
        mockDb.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
