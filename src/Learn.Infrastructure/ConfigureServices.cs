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

        // Bind options
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.Configure<ClaudeOptions>(configuration.GetSection(ClaudeOptions.SectionName));

        // Startup diagnostic: warn if Claude API key is absent so the fallback chain is transparent
        if (string.IsNullOrWhiteSpace(configuration["Claude:ApiKey"]))
            Console.WriteLine("[WARNING] Claude:ApiKey is not configured. AI grading will use Ollama fallback. Set ANTHROPIC_API_KEY env var.");

        // Register OllamaService as concrete type (used as fallback inside ClaudeAIService)
        services.AddHttpClient<OllamaService>(client =>
        {
            string baseUrl = configuration["Ollama:BaseUrl"] ?? "http://ollama:11434";
            client.BaseAddress = new Uri(baseUrl);
            int timeout = int.TryParse(configuration["Ollama:TimeoutSeconds"], out int t) ? t : 120;
            client.Timeout = TimeSpan.FromSeconds(timeout);
        });

        // Register ClaudeAIService as the primary IAIEvaluationService
        services.AddHttpClient<IAIEvaluationService, ClaudeAIService>(client =>
        {
            client.BaseAddress = new Uri("https://api.anthropic.com");
            int timeout = int.TryParse(configuration["Claude:TimeoutSeconds"], out int t) ? t : 15;
            client.Timeout = TimeSpan.FromSeconds(timeout);
        });

        services.AddScoped<IAIConnectionTester, AIConnectionTester>();
        services.AddScoped<ISpeechService, SpeechService>();
        services.AddScoped<ITopicCatalogSeeder, TopicCatalogSeeder>();

        return services;
    }
}
