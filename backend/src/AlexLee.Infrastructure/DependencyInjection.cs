using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AlexLee.Infrastructure.Data;

namespace AlexLee.Infrastructure;

/// <summary>
/// Service registration for Infrastructure layer dependencies
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Register Infrastructure layer services
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Entity Framework DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=alexlee.db";

        services.AddDbContext<AlexLeeDbContext>(options =>
        {
            options.UseSqlite(connectionString);
            options.EnableSensitiveDataLogging(false);
        });

        return services;
    }
}
