using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Learn.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace Learn.IntegrationTests;

public class AuthControllerTests : IClassFixture<AuthControllerTests.AuthWebFactory>, IDisposable
{
    private readonly HttpClient _client;

    public AuthControllerTests(AuthWebFactory factory)
    {
        _client = factory.CreateClient();
    }

    public void Dispose()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    // --- Register Tests ---

    [Fact]
    public async Task Register_WithValidCredentials_ReturnsOkWithToken()
    {
        string uniqueEmail = $"test-{Guid.NewGuid():N}@example.com";

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email = uniqueEmail,
            password = "TestPass1A"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("userId").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("email").GetString().Should().Be(uniqueEmail);
        body.GetProperty("expiresAt").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        string uniqueEmail = $"test-{Guid.NewGuid():N}@example.com";

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email = uniqueEmail,
            password = "short"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithNoUppercase_ReturnsBadRequest()
    {
        string uniqueEmail = $"test-{Guid.NewGuid():N}@example.com";

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email = uniqueEmail,
            password = "alllowercase1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        string uniqueEmail = $"test-{Guid.NewGuid():N}@example.com";

        HttpResponseMessage first = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email = uniqueEmail,
            password = "TestPass1A"
        });
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage second = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email = uniqueEmail,
            password = "TestPass1A"
        });
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        JsonElement body = await second.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("errors").EnumerateArray().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_CreatesUserStreak_AndTokenWorksForStreakEndpoint()
    {
        string uniqueEmail = $"test-{Guid.NewGuid():N}@example.com";

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email = uniqueEmail,
            password = "TestPass1A"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        string token = body.GetProperty("token").GetString()!;

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage streakResponse = await _client.GetAsync("/api/Streaks/me");
        streakResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- Login Tests ---

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        string uniqueEmail = $"test-{Guid.NewGuid():N}@example.com";
        string password = "TestPass1A";

        await _client.PostAsJsonAsync("/api/Auth/register", new { email = uniqueEmail, password });

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = uniqueEmail,
            password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("email").GetString().Should().Be(uniqueEmail);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        string uniqueEmail = $"test-{Guid.NewGuid():N}@example.com";

        await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email = uniqueEmail,
            password = "TestPass1A"
        });

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = uniqueEmail,
            password = "WrongPassword1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonexistentEmail_ReturnsUnauthorized()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = "doesnotexist@example.com",
            password = "TestPass1A"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_TokenWorksForAuthenticatedEndpoints()
    {
        string uniqueEmail = $"test-{Guid.NewGuid():N}@example.com";
        string password = "TestPass1A";

        await _client.PostAsJsonAsync("/api/Auth/register", new { email = uniqueEmail, password });

        HttpResponseMessage loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = uniqueEmail,
            password
        });

        JsonElement body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        string token = body.GetProperty("token").GetString()!;

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage topicsResponse = await _client.GetAsync("/api/Topics");
        topicsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- Google Auth Tests ---

    [Fact]
    public async Task GoogleLogin_WithInvalidToken_ReturnsUnauthorized()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Auth/google", new
        {
            idToken = "this-is-not-a-valid-google-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("error").GetString().Should().Contain("Invalid Google token");
    }

    [Fact]
    public async Task GoogleLogin_WithEmptyToken_ReturnsUnauthorized()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Auth/google", new
        {
            idToken = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- Unauthenticated access ---

    [Fact]
    public async Task AuthenticatedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Streaks/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- WebApplicationFactory ---

    public class AuthWebFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"AuthTests-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real SQL Server DbContext registration
                ServiceDescriptor? descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<LearnDbContext>));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<LearnDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });
            });

            builder.UseEnvironment("Development");
        }
    }
}
