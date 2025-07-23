using AlexLee.Application.Telemetry;
using System.Diagnostics;

namespace AlexLee.Api.Middleware;

/// <summary>
/// Middleware to capture telemetry data for all HTTP requests
/// </summary>
public class TelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITelemetryService _telemetryService;
    private readonly ILogger<TelemetryMiddleware> _logger;

    public TelemetryMiddleware(RequestDelegate next, ITelemetryService telemetryService, ILogger<TelemetryMiddleware> logger)
    {
        _next = next;
        _telemetryService = telemetryService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "";
        
        // Start activity for request tracing
        using var activity = _telemetryService.StartActivity($"HTTP {method} {path}", new Dictionary<string, object>
        {
            ["http.method"] = method,
            ["http.url"] = $"{context.Request.Scheme}://{context.Request.Host}{path}",
            ["http.user_agent"] = context.Request.Headers.UserAgent.ToString(),
            ["user.id"] = context.User?.Identity?.Name ?? "anonymous"
        });

        try
        {
            // Execute next middleware
            await _next(context);
            
            // Record successful request metrics
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            
            _telemetryService.RecordRequest(method, path, statusCode);
            _telemetryService.RecordRequestDuration(method, path, stopwatch.Elapsed.TotalMilliseconds);
            
            // Add response status to activity
            activity?.SetStatus(statusCode >= 400 ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.SetTag("http.status_code", statusCode);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Record error metrics
            _telemetryService.RecordError("HTTP", ex.GetType().Name, ex);
            _telemetryService.RecordRequest(method, path, 500);
            _telemetryService.RecordRequestDuration(method, path, stopwatch.Elapsed.TotalMilliseconds);
            
            // Mark activity as error
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("http.status_code", 500);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            
            // Log the error (this will go to our database logger for Warning+ levels)
            _logger.LogError(ex, "Unhandled exception in HTTP request {Method} {Path}", method, path);
            
            throw;
        }
    }
}