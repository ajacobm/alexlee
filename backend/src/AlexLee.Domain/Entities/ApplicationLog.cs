namespace AlexLee.Domain.Entities;

/// <summary>
/// Domain entity representing application log entries for structured logging
/// </summary>
public record ApplicationLog
{
    /// <summary>
    /// Unique identifier for the log entry
    /// </summary>
    public long Id { get; init; }
    
    /// <summary>
    /// Timestamp when the log entry was created (UTC)
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Log level (Error, Warning, Information, Debug)
    /// </summary>
    public string Level { get; init; } = null!;
    
    /// <summary>
    /// Logger category or namespace
    /// </summary>
    public string Category { get; init; } = null!;
    
    /// <summary>
    /// Log message content
    /// </summary>
    public string Message { get; init; } = null!;
    
    /// <summary>
    /// Exception details if applicable
    /// </summary>
    public string? Exception { get; init; }
    
    /// <summary>
    /// Additional properties as JSON metadata
    /// </summary>
    public string? Properties { get; init; }
    
    /// <summary>
    /// OpenTelemetry trace identifier for correlation
    /// </summary>
    public string? TraceId { get; init; }
    
    /// <summary>
    /// OpenTelemetry span identifier for correlation
    /// </summary>
    public string? SpanId { get; init; }
    
    /// <summary>
    /// User identifier if available
    /// </summary>
    public string? UserId { get; init; }
    
    /// <summary>
    /// HTTP request path if applicable
    /// </summary>
    public string? RequestPath { get; init; }
    
    /// <summary>
    /// Machine name where the log was generated
    /// </summary>
    public string MachineName { get; init; } = Environment.MachineName;
    
    /// <summary>
    /// Process identifier
    /// </summary>
    public int ProcessId { get; init; } = Environment.ProcessId;
}