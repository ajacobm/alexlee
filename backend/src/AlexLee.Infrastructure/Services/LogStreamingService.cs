using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AlexLee.Infrastructure.Services;

/// <summary>
/// Configuration for log streaming service
/// </summary>
public class LogStreamingConfiguration
{
    public int MaxConnections { get; set; } = 50;
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public int BufferSize { get; set; } = 1000;
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(2);
}

/// <summary>
/// Represents an active SSE connection for log streaming
/// </summary>
public class LogStreamConnection
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? UserId { get; set; }
    public string LogLevel { get; set; } = "Warning";
    public long LastLogId { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public Func<object, Task> SendDataAsync { get; set; } = null!;
    public CancellationToken CancellationToken { get; set; }
}

/// <summary>
/// Background service to manage log streaming connections and broadcast new log entries
/// </summary>
public class LogStreamingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LogStreamingService> _logger;
    private readonly LogStreamingConfiguration _configuration;
    private readonly ConcurrentDictionary<string, LogStreamConnection> _connections = new();

    public LogStreamingService(
        IServiceProvider serviceProvider, 
        ILogger<LogStreamingService> logger,
        LogStreamingConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Register a new SSE connection
    /// </summary>
    public string RegisterConnection(LogStreamConnection connection)
    {
        // Check connection limits
        if (_connections.Count >= _configuration.MaxConnections)
        {
            throw new InvalidOperationException($"Maximum number of connections ({_configuration.MaxConnections}) reached");
        }

        _connections.TryAdd(connection.Id, connection);
        _logger.LogInformation("Registered new log stream connection {ConnectionId} for user {UserId}", 
            connection.Id, connection.UserId ?? "anonymous");
        
        return connection.Id;
    }

    /// <summary>
    /// Unregister an SSE connection
    /// </summary>
    public void UnregisterConnection(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var connection))
        {
            _logger.LogInformation("Unregistered log stream connection {ConnectionId} for user {UserId}", 
                connectionId, connection.UserId ?? "anonymous");
        }
    }

    /// <summary>
    /// Get current connection statistics
    /// </summary>
    public object GetConnectionStats()
    {
        return new
        {
            ActiveConnections = _connections.Count,
            MaxConnections = _configuration.MaxConnections,
            Connections = _connections.Values.Select(c => new
            {
                c.Id,
                c.UserId,
                c.LogLevel,
                c.ConnectedAt,
                c.LastActivity,
                DurationMinutes = (DateTime.UtcNow - c.ConnectedAt).TotalMinutes
            }).ToList()
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Log streaming service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessLogBroadcast();
                await CleanUpStaleConnections();
                await Task.Delay(_configuration.CheckInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in log streaming service");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("Log streaming service stopped");
    }

    private async Task ProcessLogBroadcast()
    {
        if (!_connections.Any())
            return;

        using var scope = _serviceProvider.CreateScope();
        // Note: In a real implementation, we would use a more sophisticated mechanism
        // like database change notifications or a message queue to broadcast new logs
        // For now, this is a basic implementation that could be enhanced

        // Group connections by log level to optimize queries
        var connectionsByLevel = _connections.Values
            .GroupBy(c => c.LogLevel)
            .ToList();

        foreach (var levelGroup in connectionsByLevel)
        {
            var connections = levelGroup.ToList();
            var minLastLogId = connections.Min(c => c.LastLogId);
            
            // This would typically use a more efficient mechanism for real-time notifications
            // For example: SQL Server Service Broker, SignalR, or a message queue
            
            _logger.LogDebug("Broadcasting logs for level {LogLevel} to {ConnectionCount} connections",
                levelGroup.Key, connections.Count);
        }
    }

    private Task CleanUpStaleConnections()
    {
        var timeout = DateTime.UtcNow - _configuration.ConnectionTimeout;
        var staleConnections = _connections.Values
            .Where(c => c.LastActivity < timeout || c.CancellationToken.IsCancellationRequested)
            .ToList();

        foreach (var staleConnection in staleConnections)
        {
            UnregisterConnection(staleConnection.Id);
        }

        if (staleConnections.Any())
        {
            _logger.LogInformation("Cleaned up {Count} stale connections", staleConnections.Count);
        }

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _connections.Clear();
        base.Dispose();
    }
}