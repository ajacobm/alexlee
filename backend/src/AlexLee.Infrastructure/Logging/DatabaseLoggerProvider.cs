using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AlexLee.Infrastructure.Data;
using AlexLee.Domain.Entities;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Diagnostics;

namespace AlexLee.Infrastructure.Logging;

/// <summary>
/// Database logger provider that writes log entries to SQL Server
/// Filters to only log Warning and Error levels to reduce database load
/// </summary>
public sealed class DatabaseLoggerProvider : ILoggerProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DatabaseLoggerConfiguration _configuration;
    private readonly ConcurrentDictionary<string, DatabaseLogger> _loggers = new();
    private readonly DatabaseLoggerProcessor _processor;
    private bool _disposed;

    public DatabaseLoggerProvider(IServiceProvider serviceProvider, DatabaseLoggerConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _processor = new DatabaseLoggerProcessor(serviceProvider, configuration);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => 
            new DatabaseLogger(name, _processor, _configuration));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _processor.Dispose();
            _loggers.Clear();
            _disposed = true;
        }
    }
}

/// <summary>
/// Configuration for database logger
/// </summary>
public class DatabaseLoggerConfiguration
{
    public LogLevel MinimumLevel { get; set; } = LogLevel.Warning;
    public int BufferSize { get; set; } = 100;
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(30);
    public bool IncludeScopes { get; set; } = true;
    public bool IncludeEventId { get; set; } = true;
}

/// <summary>
/// Database logger implementation
/// </summary>
internal sealed class DatabaseLogger : ILogger
{
    private readonly string _categoryName;
    private readonly DatabaseLoggerProcessor _processor;
    private readonly DatabaseLoggerConfiguration _configuration;

    public DatabaseLogger(string categoryName, DatabaseLoggerProcessor processor, DatabaseLoggerConfiguration configuration)
    {
        _categoryName = categoryName;
        _processor = processor;
        _configuration = configuration;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _configuration.IncludeScopes ? new DatabaseLoggerScope(state) : null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // Only log Warning and above to database to reduce load
        return logLevel >= _configuration.MinimumLevel && logLevel != LogLevel.None;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null)
            return;

        var logEntry = CreateLogEntry(logLevel, eventId, message, exception);
        _processor.EnqueueLogEntry(logEntry);
    }

    private ApplicationLog CreateLogEntry(LogLevel logLevel, EventId eventId, string message, Exception? exception)
    {
        var activity = Activity.Current;
        var properties = new Dictionary<string, object>();

        if (_configuration.IncludeEventId && eventId.Id != 0)
        {
            properties["EventId"] = eventId.Id;
            if (!string.IsNullOrEmpty(eventId.Name))
                properties["EventName"] = eventId.Name;
        }

        // Add any additional context from current activity
        if (activity != null)
        {
            foreach (var tag in activity.Tags)
            {
                properties[tag.Key] = tag.Value ?? "";
            }
        }

        return new ApplicationLog
        {
            Timestamp = DateTime.UtcNow,
            Level = logLevel.ToString(),
            Category = _categoryName,
            Message = message,
            Exception = exception?.ToString(),
            Properties = properties.Count > 0 ? JsonSerializer.Serialize(properties) : null,
            TraceId = activity?.TraceId.ToString(),
            SpanId = activity?.SpanId.ToString(),
            // RequestPath and UserId would be populated by middleware
        };
    }
}

/// <summary>
/// Processes log entries in batches using a background service
/// </summary>
internal sealed class DatabaseLoggerProcessor : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DatabaseLoggerConfiguration _configuration;
    private readonly ConcurrentQueue<ApplicationLog> _logQueue = new();
    private readonly Timer _flushTimer;
    private readonly SemaphoreSlim _flushSemaphore = new(1, 1);
    private bool _disposed;

    public DatabaseLoggerProcessor(IServiceProvider serviceProvider, DatabaseLoggerConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        
        // Set up periodic flushing
        _flushTimer = new Timer(FlushLogs, null, _configuration.FlushInterval, _configuration.FlushInterval);
    }

    public void EnqueueLogEntry(ApplicationLog logEntry)
    {
        if (_disposed)
            return;

        _logQueue.Enqueue(logEntry);

        // If buffer is full, trigger immediate flush
        if (_logQueue.Count >= _configuration.BufferSize)
        {
            _ = Task.Run(async () => await FlushLogsAsync());
        }
    }

    private void FlushLogs(object? state)
    {
        _ = Task.Run(async () => await FlushLogsAsync());
    }

    private async Task FlushLogsAsync()
    {
        if (_disposed || !await _flushSemaphore.WaitAsync(1000))
            return;

        try
        {
            var logsToWrite = new List<ApplicationLog>();
            
            // Dequeue all pending logs
            while (_logQueue.TryDequeue(out var log) && logsToWrite.Count < _configuration.BufferSize)
            {
                logsToWrite.Add(log);
            }

            if (logsToWrite.Count == 0)
                return;

            // Write to database
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AlexLeeDbContext>();

            try
            {
                await dbContext.ApplicationLogs.AddRangeAsync(logsToWrite);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Avoid infinite recursion - use console logging for database logger errors
                Console.WriteLine($"[DatabaseLogger] Error writing logs to database: {ex.Message}");
            }
        }
        finally
        {
            _flushSemaphore.Release();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _flushTimer?.Dispose();
            
            // Final flush
            _ = Task.Run(async () => await FlushLogsAsync());
            
            _flushSemaphore.Dispose();
        }
    }
}

/// <summary>
/// Logger scope implementation
/// </summary>
internal sealed class DatabaseLoggerScope : IDisposable
{
    private readonly object _state;

    public DatabaseLoggerScope(object state)
    {
        _state = state;
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    public override string ToString()
    {
        return _state?.ToString() ?? string.Empty;
    }
}