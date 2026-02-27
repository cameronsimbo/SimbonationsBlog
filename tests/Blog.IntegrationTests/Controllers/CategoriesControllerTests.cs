using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Blog.IntegrationTests.Controllers;

public class CategoriesControllerTests : IClassFixture<BlogWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriesControllerTests(BlogWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        object command = new { Name = "Tech", Description = "Technology articles" };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/categories", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
