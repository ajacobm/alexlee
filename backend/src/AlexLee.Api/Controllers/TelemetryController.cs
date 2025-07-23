using Microsoft.AspNetCore.Mvc;
using AlexLee.Infrastructure.Data;
using AlexLee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Diagnostics;
using System.Text;

namespace AlexLee.Api.Controllers;

/// <summary>
/// Controller for telemetry data access and streaming
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TelemetryController : ControllerBase
{
    private readonly AlexLeeDbContext _context;
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(AlexLeeDbContext context, ILogger<TelemetryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get current application metrics in Prometheus format
    /// </summary>
    /// <returns>Prometheus-compatible metrics text</returns>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        try
        {
            var metrics = new StringBuilder();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Database statistics
            var totalLogs = await _context.ApplicationLogs.CountAsync();
            var totalPurchaseDetails = await _context.PurchaseDetailItems.CountAsync();
            
            // Recent error count (last hour)
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            var recentErrors = await _context.ApplicationLogs
                .Where(l => l.Level == "Error" && l.Timestamp >= oneHourAgo)
                .CountAsync();

            // Recent warning count (last hour)  
            var recentWarnings = await _context.ApplicationLogs
                .Where(l => l.Level == "Warning" && l.Timestamp >= oneHourAgo)
                .CountAsync();

            // System metrics
            var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var cpuTime = process.TotalProcessorTime.TotalMilliseconds;

            // Build Prometheus format metrics
            metrics.AppendLine("# HELP alexlee_database_logs_total Total number of log entries in database");
            metrics.AppendLine("# TYPE alexlee_database_logs_total counter");
            metrics.AppendLine($"alexlee_database_logs_total {totalLogs} {timestamp}");
            
            metrics.AppendLine("# HELP alexlee_purchase_details_total Total number of purchase details in database");
            metrics.AppendLine("# TYPE alexlee_purchase_details_total counter");
            metrics.AppendLine($"alexlee_purchase_details_total {totalPurchaseDetails} {timestamp}");
            
            metrics.AppendLine("# HELP alexlee_errors_recent_total Recent errors in the last hour");
            metrics.AppendLine("# TYPE alexlee_errors_recent_total counter");
            metrics.AppendLine($"alexlee_errors_recent_total {recentErrors} {timestamp}");
            
            metrics.AppendLine("# HELP alexlee_warnings_recent_total Recent warnings in the last hour");
            metrics.AppendLine("# TYPE alexlee_warnings_recent_total counter");
            metrics.AppendLine($"alexlee_warnings_recent_total {recentWarnings} {timestamp}");
            
            metrics.AppendLine("# HELP alexlee_memory_working_set_bytes Current working set memory usage");
            metrics.AppendLine("# TYPE alexlee_memory_working_set_bytes gauge");
            metrics.AppendLine($"alexlee_memory_working_set_bytes {workingSet} {timestamp}");
            
            metrics.AppendLine("# HELP alexlee_cpu_total_milliseconds Total CPU time used");
            metrics.AppendLine("# TYPE alexlee_cpu_total_milliseconds counter");
            metrics.AppendLine($"alexlee_cpu_total_milliseconds {cpuTime} {timestamp}");

            return Content(metrics.ToString(), "text/plain; charset=utf-8");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics");
            return StatusCode(500, "Error retrieving metrics");
        }
    }

    /// <summary>
    /// Stream logs in real-time using Server-Sent Events (SSE)
    /// </summary>
    /// <param name="level">Minimum log level to stream (default: Warning)</param>
    /// <param name="maxLines">Maximum number of lines to initially send (default: 50)</param>
    /// <returns>SSE stream of log entries</returns>
    [HttpGet("logs/stream")]
    public async Task StreamLogs([FromQuery] string level = "Warning", [FromQuery] int maxLines = 50)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("Access-Control-Allow-Origin", "*");
        Response.Headers.Append("Access-Control-Allow-Headers", "Cache-Control");

        try
        {
            _logger.LogInformation("Starting log stream for level {Level} with max lines {MaxLines}", 
                level, maxLines);

            // Send initial batch of recent logs
            var recentLogs = await _context.ApplicationLogs
                .Where(l => string.IsNullOrEmpty(level) || l.Level == level || 
                           (level == "Warning" && (l.Level == "Warning" || l.Level == "Error")))
                .OrderByDescending(l => l.Timestamp)
                .Take(maxLines)
                .Select(l => new
                {
                    l.Id,
                    l.Timestamp,
                    l.Level,
                    l.Category,
                    l.Message,
                    l.Exception,
                    l.TraceId,
                    l.SpanId,
                    l.RequestPath,
                    l.UserId
                })
                .ToListAsync();

            // Send recent logs in reverse order (oldest first)
            foreach (var log in recentLogs.Reverse<object>())
            {
                var data = JsonSerializer.Serialize(log);
                await Response.WriteAsync($"data: {data}\n\n");
                await Response.Body.FlushAsync();
            }

            // Keep connection alive and send heartbeat every 30 seconds
            var cancellationToken = HttpContext.RequestAborted;
            var lastLogId = recentLogs.FirstOrDefault()?.GetType().GetProperty("Id")?.GetValue(recentLogs.First()) as long? ?? 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Check for new logs since last send
                    var newLogs = await _context.ApplicationLogs
                        .Where(l => l.Id > lastLogId &&
                                   (string.IsNullOrEmpty(level) || l.Level == level || 
                                   (level == "Warning" && (l.Level == "Warning" || l.Level == "Error"))))
                        .OrderBy(l => l.Timestamp)
                        .Select(l => new
                        {
                            l.Id,
                            l.Timestamp,
                            l.Level,
                            l.Category,
                            l.Message,
                            l.Exception,
                            l.TraceId,
                            l.SpanId,
                            l.RequestPath,
                            l.UserId
                        })
                        .ToListAsync(cancellationToken);

                    // Send new logs
                    foreach (var log in newLogs)
                    {
                        lastLogId = log.Id;
                        var data = JsonSerializer.Serialize(log);
                        await Response.WriteAsync($"data: {data}\n\n", cancellationToken);
                        await Response.Body.FlushAsync(cancellationToken);
                    }

                    // Send heartbeat if no new logs
                    if (!newLogs.Any())
                    {
                        var heartbeat = JsonSerializer.Serialize(new { 
                            type = "heartbeat", 
                            timestamp = DateTime.UtcNow,
                            activeConnections = "1" // Could track this globally
                        });
                        await Response.WriteAsync($"event: heartbeat\ndata: {heartbeat}\n\n", cancellationToken);
                        await Response.Body.FlushAsync(cancellationToken);
                    }

                    // Wait before next check (2 seconds for new logs, 30 seconds for heartbeat)
                    await Task.Delay(newLogs.Any() ? 2000 : 30000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in log streaming");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting log stream");
            await Response.WriteAsync($"event: error\ndata: {{\"error\": \"Error starting stream\"}}\n\n");
        }

        _logger.LogInformation("Log stream ended");
    }

    /// <summary>
    /// Get paginated historical logs
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="size">Page size (default: 100)</param>
    /// <param name="level">Filter by log level</param>
    /// <param name="category">Filter by category</param>
    /// <param name="fromDate">Filter logs from this date</param>
    /// <param name="toDate">Filter logs to this date</param>
    /// <returns>Paginated log entries</returns>
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string? level = null,
        [FromQuery] string? category = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (size < 1 || size > 1000) size = 100;

            var query = _context.ApplicationLogs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(level))
            {
                if (level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(l => l.Level == "Warning" || l.Level == "Error");
                }
                else
                {
                    query = query.Where(l => l.Level == level);
                }
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(l => l.Category.Contains(category));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.Timestamp <= toDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Get paginated results
            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(l => new
                {
                    l.Id,
                    l.Timestamp,
                    l.Level,
                    l.Category,
                    l.Message,
                    l.Exception,
                    l.TraceId,
                    l.SpanId,
                    l.RequestPath,
                    l.UserId,
                    l.MachineName,
                    l.ProcessId
                })
                .ToListAsync();

            var response = new
            {
                Data = logs,
                Pagination = new
                {
                    Page = page,
                    Size = size,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / size),
                    HasNextPage = page * size < totalCount,
                    HasPreviousPage = page > 1
                },
                Filters = new
                {
                    Level = level,
                    Category = category,
                    FromDate = fromDate,
                    ToDate = toDate
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs");
            return StatusCode(500, "Error retrieving logs");
        }
    }

    /// <summary>
    /// Get log statistics summary
    /// </summary>
    /// <returns>Log statistics including counts by level and recent trends</returns>
    [HttpGet("stats")]
    public async Task<IActionResult> GetLogStatistics()
    {
        try
        {
            var now = DateTime.UtcNow;
            var oneHourAgo = now.AddHours(-1);
            var oneDayAgo = now.AddDays(-1);
            var oneWeekAgo = now.AddDays(-7);

            var stats = new
            {
                TotalLogs = await _context.ApplicationLogs.CountAsync(),
                LogsByLevel = await _context.ApplicationLogs
                    .GroupBy(l => l.Level)
                    .Select(g => new { Level = g.Key, Count = g.Count() })
                    .ToListAsync(),
                RecentActivity = new
                {
                    LastHour = new
                    {
                        Total = await _context.ApplicationLogs
                            .Where(l => l.Timestamp >= oneHourAgo)
                            .CountAsync(),
                        Errors = await _context.ApplicationLogs
                            .Where(l => l.Level == "Error" && l.Timestamp >= oneHourAgo)
                            .CountAsync(),
                        Warnings = await _context.ApplicationLogs
                            .Where(l => l.Level == "Warning" && l.Timestamp >= oneHourAgo)
                            .CountAsync()
                    },
                    LastDay = new
                    {
                        Total = await _context.ApplicationLogs
                            .Where(l => l.Timestamp >= oneDayAgo)
                            .CountAsync(),
                        Errors = await _context.ApplicationLogs
                            .Where(l => l.Level == "Error" && l.Timestamp >= oneDayAgo)
                            .CountAsync(),
                        Warnings = await _context.ApplicationLogs
                            .Where(l => l.Level == "Warning" && l.Timestamp >= oneDayAgo)
                            .CountAsync()
                    },
                    LastWeek = new
                    {
                        Total = await _context.ApplicationLogs
                            .Where(l => l.Timestamp >= oneWeekAgo)
                            .CountAsync(),
                        Errors = await _context.ApplicationLogs
                            .Where(l => l.Level == "Error" && l.Timestamp >= oneWeekAgo)
                            .CountAsync(),
                        Warnings = await _context.ApplicationLogs
                            .Where(l => l.Level == "Warning" && l.Timestamp >= oneWeekAgo)
                            .CountAsync()
                    }
                },
                TopCategories = await _context.ApplicationLogs
                    .Where(l => l.Timestamp >= oneWeekAgo)
                    .GroupBy(l => l.Category)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync(),
                LastUpdated = now
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving log statistics");
            return StatusCode(500, "Error retrieving log statistics");
        }
    }
}