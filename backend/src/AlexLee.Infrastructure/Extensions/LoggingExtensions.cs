using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlexLee.Infrastructure.Logging;

namespace AlexLee.Infrastructure.Extensions;

/// <summary>
/// Extension methods for logging configuration
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds database logging to the logging builder
    /// </summary>
    public static ILoggingBuilder AddDatabaseLogger(this ILoggingBuilder builder, Action<DatabaseLoggerConfiguration>? configureOptions = null)
    {
        var configuration = new DatabaseLoggerConfiguration();
        configureOptions?.Invoke(configuration);

        builder.Services.AddSingleton(configuration);
        builder.Services.AddSingleton<ILoggerProvider, DatabaseLoggerProvider>();
        
        return builder;
    }

    /// <summary>
    /// Adds database logging to the logging builder with specific configuration
    /// </summary>
    public static ILoggingBuilder AddDatabaseLogger(this ILoggingBuilder builder, DatabaseLoggerConfiguration configuration)
    {
        builder.Services.AddSingleton(configuration);
        builder.Services.AddSingleton<ILoggerProvider, DatabaseLoggerProvider>();
        
        return builder;
    }
}