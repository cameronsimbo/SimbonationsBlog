using Blog.Application.Common.Interfaces;
using Blog.Infrastructure;
using Blog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BlogDbContext>(options =>
            options.UseSqlServer(connectionString, builder =>
                builder.MigrationsAssembly(typeof(BlogDbContext).Assembly.FullName)));

        services.AddScoped<IBlogDbContext>(provider => provider.GetRequiredService<BlogDbContext>());

        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}
