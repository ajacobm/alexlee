using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using AlexLee.Application.Telemetry;

namespace AlexLee.Application;

/// <summary>
/// Service registration for Application layer dependencies
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Register Application layer services
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR for CQRS pattern
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register telemetry service
        services.AddSingleton<ITelemetryService, TelemetryService>();

        return services;
    }
}
