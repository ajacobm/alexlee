# Telemetry and Logging Implementation Plan
*Alex Lee Developer Exercise - Service Tier Enhancement*

## Executive Summary

This plan outlines the implementation of comprehensive telemetry, database logging, and real-time console monitoring for our .NET 8 service tier with React frontend integration. The solution leverages OpenTelemetry standards for Azure Application Insights compatibility and includes a real-time streaming console component.

## 🎯 Objectives

1. **OpenTelemetry Integration**: Implement industry-standard telemetry with Azure Application Insights compatibility
2. **Database Logging**: Add structured error and warning logging to SQL Server Express
3. **Real-time Console**: Create a streaming frontend component for live metrics monitoring
4. **Architectural Compliance**: Maintain existing CQRS, DI, and clean architecture patterns
5. **Production Readiness**: Ensure scalable, secure, and performant implementation

## 📋 Implementation Phases

### Phase 1: Database Logging Infrastructure
**Duration**: 2-3 hours  
**Risk Level**: Low

#### 1.1 Log Table Schema
```sql
-- New table in AlexLeeDbContext
CREATE TABLE [dbo].[ApplicationLogs] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [Timestamp] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [Level] NVARCHAR(50) NOT NULL, -- Error, Warning, Information, Debug
    [Category] NVARCHAR(200) NOT NULL, -- Logger category
    [Message] NVARCHAR(MAX) NOT NULL,
    [Exception] NVARCHAR(MAX) NULL,
    [Properties] NVARCHAR(MAX) NULL, -- JSON metadata
    [TraceId] NVARCHAR(32) NULL, -- OpenTelemetry trace correlation
    [SpanId] NVARCHAR(16) NULL,
    [UserId] NVARCHAR(100) NULL,
    [RequestPath] NVARCHAR(500) NULL,
    [MachineName] NVARCHAR(100) NOT NULL DEFAULT HOST_NAME(),
    [ProcessId] INT NOT NULL DEFAULT @@SPID
);

CREATE INDEX IX_ApplicationLogs_Level_Timestamp ON ApplicationLogs(Level, Timestamp DESC);
CREATE INDEX IX_ApplicationLogs_Category_Timestamp ON ApplicationLogs(Category, Timestamp DESC);
CREATE INDEX IX_ApplicationLogs_TraceId ON ApplicationLogs(TraceId);
```

#### 1.2 Domain Entity
```csharp
// AlexLee.Domain/Entities/ApplicationLog.cs
public record ApplicationLog
{
    public long Id { get; init; }
    public DateTime Timestamp { get; init; }
    public string Level { get; init; } = null!;
    public string Category { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? Exception { get; init; }
    public string? Properties { get; init; }
    public string? TraceId { get; init; }
    public string? SpanId { get; init; }
    public string? UserId { get; init; }
    public string? RequestPath { get; init; }
    public string MachineName { get; init; } = null!;
    public int ProcessId { get; init; }
}
```

#### 1.3 Custom Database Logger Provider
```csharp
// AlexLee.Infrastructure/Logging/DatabaseLoggerProvider.cs
public sealed class DatabaseLoggerProvider : ILoggerProvider
{
    private readonly IDbContextFactory<AlexLeeDbContext> _contextFactory;
    private readonly ConcurrentDictionary<string, DatabaseLogger> _loggers = new();
    
    // Implements batched, async logging with background service
    // Filters to only log Warning and Error levels to database
    // Includes OpenTelemetry trace correlation
}
```

### Phase 2: OpenTelemetry Integration  
**Duration**: 4-5 hours  
**Risk Level**: Medium

#### 2.1 NuGet Package Dependencies
```xml
<!-- Add to AlexLee.Api.csproj -->
<PackageReference Include="OpenTelemetry" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.12" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.SqlClient" Version="1.9.0-beta.1" />
<PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.9.0" />
<PackageReference Include="Microsoft.Extensions.Logging.ApplicationInsights" Version="2.22.3" />
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.3" />
```

#### 2.2 Telemetry Configuration
```csharp
// Program.cs enhancement
builder.Services.AddOpenTelemetry()
    .WithTracing(builder =>
    {
        builder
            .AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation(options => 
            {
                options.SetDbStatementForText = true;
                options.RecordException = true;
            })
            .SetSampler(new AlwaysOnSampler())
            .AddConsoleExporter()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
            });
    })
    .WithMetrics(builder =>
    {
        builder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddConsoleExporter()
            .AddOtlpExporter();
    });

// Azure Application Insights integration
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
    options.EnableAdaptiveSampling = true;
    options.EnableHeartbeat = true;
    options.EnableDebugLogger = builder.Environment.IsDevelopment();
});
```

#### 2.3 Custom Metrics and Activities
```csharp
// AlexLee.Application/Telemetry/TelemetryService.cs
public class TelemetryService : ITelemetryService
{
    private static readonly ActivitySource ActivitySource = new("AlexLee.Application");
    private static readonly Meter Meter = new("AlexLee.Application");
    
    // Custom metrics
    private readonly Counter<long> _requestCounter = Meter.CreateCounter<long>("alexlee.requests.total");
    private readonly Histogram<double> _requestDuration = Meter.CreateHistogram<double>("alexlee.requests.duration");
    private readonly Counter<long> _errorCounter = Meter.CreateCounter<long>("alexlee.errors.total");
    
    // Business-specific metrics
    private readonly Counter<long> _purchaseDetailsCreated = Meter.CreateCounter<long>("alexlee.purchase_details.created");
    private readonly Counter<long> _algorithmsExecuted = Meter.CreateCounter<long>("alexlee.algorithms.executed");
}
```

### Phase 3: Telemetry Streaming API
**Duration**: 3-4 hours  
**Risk Level**: Medium-High

#### 3.1 Real-time Metrics Controller
```csharp
// AlexLee.Api/Controllers/TelemetryController.cs
[Route("api/[controller]")]
public class TelemetryController : ControllerBase
{
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        // Prometheus-compatible metrics endpoint
        // Returns current application metrics in text/plain format
    }
    
    [HttpGet("logs/stream")]
    public async Task StreamLogs([FromQuery] string level = "Warning")
    {
        // Server-Sent Events (SSE) endpoint for real-time log streaming
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        
        // Stream last 50 logs + real-time updates using IChangeToken
    }
    
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string level = "",
        [FromQuery] DateTime? fromDate = null)
    {
        // Paginated historical logs API
    }
}
```

#### 3.2 Background Log Streaming Service
```csharp
// AlexLee.Infrastructure/Services/LogStreamingService.cs
public class LogStreamingService : BackgroundService
{
    private readonly ConcurrentDictionary<string, StreamWriter> _activeStreams = new();
    private readonly IChangeTokenSource<ApplicationLog> _changeTokenSource;
    
    // Manages active SSE connections
    // Broadcasts new log entries to connected clients
    // Handles connection cleanup and error recovery
}
```

### Phase 4: Frontend Console Component
**Duration**: 5-6 hours  
**Risk Level**: Medium

#### 4.1 Real-time Console Component
```typescript
// frontend/src/components/TelemetryConsole.tsx
interface LogEntry {
  id: number;
  timestamp: string;
  level: 'Error' | 'Warning' | 'Information' | 'Debug';
  category: string;
  message: string;
  exception?: string;
  properties?: Record<string, any>;
  traceId?: string;
}

interface TelemetryConsoleProps {
  maxLines?: number;
  autoScroll?: boolean;
  filterLevel?: string;
}

export const TelemetryConsole: React.FC<TelemetryConsoleProps> = ({
  maxLines = 500,
  autoScroll = true,
  filterLevel = 'Warning'
}) => {
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [isConnected, setIsConnected] = useState(false);
  const [isPaused, setIsPaused] = useState(false);
  
  // Server-Sent Events connection for real-time updates
  // Virtual scrolling for performance with large log volumes
  // Filtering, search, and export capabilities
  // Color-coded log levels with syntax highlighting
  
  return (
    <div className="telemetry-console">
      <ConsoleHeader />
      <ConsoleFilters />
      <VirtualizedLogDisplay logs={filteredLogs} />
      <ConsoleControls />
    </div>
  );
};
```

#### 4.2 Telemetry Dashboard Page
```typescript
// frontend/src/pages/TelemetryPage.tsx
export const TelemetryPage: React.FC = () => {
  return (
    <div className="telemetry-page">
      <div className="telemetry-grid">
        <MetricsSummaryCard />
        <SystemHealthCard />
        <RecentErrorsCard />
        <TelemetryConsole maxLines={300} />
      </div>
    </div>
  );
};
```

#### 4.3 Navigation Integration
```typescript
// Update Layout.tsx to include Telemetry tab
<Link 
  to="/telemetry" 
  className={`nav-link ${isActive('/telemetry') ? 'active' : ''}`}
>
  Telemetry
</Link>
```

## ⚠️ Risks and Mitigation Strategies

### High-Risk Areas

#### 1. **Database Performance Impact**
- **Risk**: Excessive logging could impact database performance
- **Mitigation**: 
  - Batched async logging with background service
  - Configurable log level filtering (Warning+ only to DB)
  - Automatic log rotation/cleanup policies
  - Connection pooling for logging operations

#### 2. **Memory Pressure from SSE Connections**
- **Risk**: Multiple streaming connections could cause memory leaks
- **Mitigation**:
  - Connection limits and timeout policies
  - Graceful connection cleanup on client disconnect
  - Memory monitoring and alerting
  - Circuit breaker pattern for overload protection

#### 3. **Frontend Performance with Large Log Volumes**
- **Risk**: Rendering thousands of log entries could freeze UI
- **Mitigation**:
  - Virtual scrolling implementation
  - Client-side log buffer limits (500-1000 entries)
  - Efficient React rendering with useMemo/useCallback
  - Web Worker for log processing if needed

### Medium-Risk Areas

#### 4. **OpenTelemetry Configuration Complexity**
- **Risk**: Misconfigured sampling or excessive telemetry overhead
- **Mitigation**:
  - Environment-specific configuration
  - Gradual rollout with performance monitoring
  - Sampling rate adjustment capabilities
  - Clear documentation and examples

#### 5. **Cross-Origin Issues with SSE**
- **Risk**: CORS complications with Server-Sent Events
- **Mitigation**:
  - Explicit CORS configuration for EventSource
  - Testing across different deployment scenarios
  - Fallback to polling if SSE fails

## 🔧 Technical Implementation Details

### Database Schema Migration

```csharp
// EF Core Migration
public partial class AddApplicationLogsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApplicationLogs",
            columns: table => new
            {
                Id = table.Column<long>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                // ... complete schema
            });
    }
}
```

### Configuration Enhancements

```json
// appsettings.json
{
  "ConnectionStrings": {
    "ApplicationInsights": "InstrumentationKey=your-key-here"
  },
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317",
    "ServiceName": "AlexLee.Api",
    "ServiceVersion": "1.0.0"
  },
  "Logging": {
    "Database": {
      "MinimumLevel": "Warning",
      "BufferSize": 100,
      "FlushInterval": "00:00:30"
    }
  },
  "Telemetry": {
    "Streaming": {
      "MaxConnections": 50,
      "ConnectionTimeout": "00:05:00",
      "BufferSize": 1000
    }
  }
}
```

## 📊 Success Metrics

### Functional Requirements Met
- ✅ OpenTelemetry-compatible metrics export
- ✅ Azure Application Insights integration
- ✅ Real-time log streaming to frontend
- ✅ Database error/warning persistence
- ✅ Console-like UI component
- ✅ Architectural pattern compliance

### Performance Targets
- Database logging: < 10ms average latency
- SSE connection setup: < 2s
- Frontend console rendering: < 100ms for 1000 entries
- Memory usage: < 50MB additional overhead
- CPU overhead: < 5% in normal operation

### Monitoring and Alerting
- Database log table growth monitoring
- SSE connection count tracking
- Application performance impact measurement
- Error rate and telemetry health checks

## 🚀 Deployment Strategy

### Development Environment
1. Local SQL Server Express (existing container)
2. Console exporter for immediate telemetry visibility
3. Hot reload for frontend console development

### Production Environment  
1. Azure Application Insights integration
2. Database logging with retention policies
3. Load balancer affinity for SSE connections
4. Health checks for telemetry endpoints

## 📅 Implementation Timeline

**Week 1**:
- Day 1-2: Database logging infrastructure
- Day 3-4: OpenTelemetry basic setup  
- Day 5: Testing and integration

**Week 2**:
- Day 1-2: Streaming API development
- Day 3-4: Frontend console component
- Day 5: E2E testing and optimization

**Week 3**:
- Day 1-2: Azure Application Insights integration
- Day 3-4: Performance testing and tuning
- Day 5: Documentation and deployment

## ✅ Definition of Done

- [ ] Database `ApplicationLogs` table created and seeded
- [ ] Custom `ILogger` provider writing to database  
- [ ] OpenTelemetry tracing and metrics configured
- [ ] Azure Application Insights compatibility verified
- [ ] SSE streaming endpoint functional
- [ ] React console component with real-time updates
- [ ] Navigation tab added to frontend
- [ ] CQRS/DI patterns maintained throughout
- [ ] Unit tests for new components
- [ ] Performance benchmarks documented
- [ ] Docker containers updated and tested
- [ ] Documentation updated in README/API docs

## 🎯 Post-Implementation Verification

### Acceptance Tests
1. **Telemetry Export**: Verify metrics appear in external telemetry systems
2. **Real-time Streaming**: Console shows live log updates within 5 seconds
3. **Database Persistence**: Error logs appear in ApplicationLogs table
4. **Performance**: No degradation in existing API response times
5. **Azure Compatibility**: Application Insights receives structured telemetry

### Integration Testing
- Full-stack telemetry flow testing
- Concurrent user streaming simulation
- Database connection pool validation
- Memory leak detection over 24-hour period

This implementation plan provides enterprise-grade telemetry capabilities while maintaining the existing clean architecture patterns and ensuring production readiness with comprehensive risk mitigation strategies.