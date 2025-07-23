using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AlexLee.Application.Telemetry;

/// <summary>
/// Interface for telemetry service
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Start a new activity with specified name and tags
    /// </summary>
    Activity? StartActivity(string name, Dictionary<string, object>? tags = null);
    
    /// <summary>
    /// Record a request counter increment
    /// </summary>
    void RecordRequest(string method, string endpoint, int statusCode);
    
    /// <summary>
    /// Record request duration
    /// </summary>
    void RecordRequestDuration(string method, string endpoint, double durationMs);
    
    /// <summary>
    /// Record an error occurrence
    /// </summary>
    void RecordError(string category, string errorType, Exception? exception = null);
    
    /// <summary>
    /// Record business metric - purchase detail created
    /// </summary>
    void RecordPurchaseDetailCreated();
    
    /// <summary>
    /// Record business metric - algorithm executed
    /// </summary>
    void RecordAlgorithmExecuted(string algorithmType);
}

/// <summary>
/// Telemetry service implementation using OpenTelemetry APIs
/// </summary>
public class TelemetryService : ITelemetryService
{
    private static readonly ActivitySource ActivitySource = new("AlexLee.Application", "1.0.0");
    private static readonly Meter Meter = new("AlexLee.Application", "1.0.0");
    
    // Custom metrics
    private readonly Counter<long> _requestCounter;
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _errorCounter;
    
    // Business-specific metrics
    private readonly Counter<long> _purchaseDetailsCreated;
    private readonly Counter<long> _algorithmsExecuted;

    public TelemetryService()
    {
        // Initialize counters and histograms
        _requestCounter = Meter.CreateCounter<long>(
            name: "alexlee.requests.total",
            description: "Total number of requests processed");
        
        _requestDuration = Meter.CreateHistogram<double>(
            name: "alexlee.requests.duration",
            unit: "ms",
            description: "Duration of API requests in milliseconds");
        
        _errorCounter = Meter.CreateCounter<long>(
            name: "alexlee.errors.total",
            description: "Total number of errors encountered");
        
        _purchaseDetailsCreated = Meter.CreateCounter<long>(
            name: "alexlee.purchase_details.created",
            description: "Number of purchase details created");
        
        _algorithmsExecuted = Meter.CreateCounter<long>(
            name: "alexlee.algorithms.executed",
            description: "Number of algorithms executed");
    }

    public Activity? StartActivity(string name, Dictionary<string, object>? tags = null)
    {
        var activity = ActivitySource.StartActivity(name);
        
        if (activity != null && tags != null)
        {
            foreach (var tag in tags)
            {
                activity.SetTag(tag.Key, tag.Value?.ToString());
            }
        }
        
        return activity;
    }

    public void RecordRequest(string method, string endpoint, int statusCode)
    {
        _requestCounter.Add(1, 
            new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("status_code", statusCode));
    }

    public void RecordRequestDuration(string method, string endpoint, double durationMs)
    {
        _requestDuration.Record(durationMs,
            new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("endpoint", endpoint));
    }

    public void RecordError(string category, string errorType, Exception? exception = null)
    {
        _errorCounter.Add(1,
            new KeyValuePair<string, object?>("category", category),
            new KeyValuePair<string, object?>("error_type", errorType),
            new KeyValuePair<string, object?>("exception_type", exception?.GetType().Name ?? "unknown"));
    }

    public void RecordPurchaseDetailCreated()
    {
        _purchaseDetailsCreated.Add(1);
    }

    public void RecordAlgorithmExecuted(string algorithmType)
    {
        _algorithmsExecuted.Add(1,
            new KeyValuePair<string, object?>("algorithm_type", algorithmType));
    }
}