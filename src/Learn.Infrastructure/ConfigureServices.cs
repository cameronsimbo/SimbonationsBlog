using Learn.Application.Common.Interfaces;
using Learn.Infrastructure.Persistence;
using Learn.Infrastructure.Seeding;
using Learn.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.AddDbContext<LearnDbContext>(options =>
            options.UseSqlServer(connectionString, builder =>
                builder.MigrationsAssembly(typeof(LearnDbContext).Assembly.FullName)));

        services.AddScoped<ILearnDbContext>(provider => provider.GetRequiredService<LearnDbContext>());

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<LearnDbContext>()
            .AddDefaultTokenProviders();

        // Bind Ollama options from configuration
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));

        // Register OllamaService with a named HttpClient
        services.AddHttpClient<IAIEvaluationService, OllamaService>(client =>
        {
            var ollamaSection = configuration.GetSection(OllamaOptions.SectionName);
            string baseUrl = ollamaSection["BaseUrl"] ?? "http://ollama:11434";
            client.BaseAddress = new Uri(baseUrl);
            int timeoutSeconds = int.TryParse(ollamaSection["TimeoutSeconds"], out int t) ? t : 120;
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        services.AddScoped<ISpeechService, SpeechService>();
        services.AddScoped<ITopicCatalogSeeder, TopicCatalogSeeder>();

        return services;
    }
}
