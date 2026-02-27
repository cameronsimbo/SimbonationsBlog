using Learn.Application.Common.Interfaces;
using Learn.Infrastructure.Persistence;
using Learn.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
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

        services.AddScoped<IAIEvaluationService, AIEvaluationService>();
        services.AddScoped<ISpeechService, SpeechService>();

        return services;
    }
}
