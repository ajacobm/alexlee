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
        // Register Entity Framework DbContext for SQL Server
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost,1433;Database=AlexLeeDB;User Id=SA;Password=P@ssw0rd123!;TrustServerCertificate=true";

        services.AddDbContext<AlexLeeDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.EnableSensitiveDataLogging(false);
        });

        return services;
    }
}