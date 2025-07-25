using AlexLee.Application;
using AlexLee.Infrastructure;
using AlexLee.Infrastructure.Data;
using AlexLee.Infrastructure.Extensions;
using AlexLee.Api.Middleware;
using Microsoft.OpenApi.Models;
using System.Reflection;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Alex Lee Developer Exercise API",
        Version = "v1.0",
        Description = "REST API for Purchase Detail Management System with SQL Server Express and OpenTelemetry",
        Contact = new OpenApiContact
        {
            Name = "Alex Lee Developer Exercise",
            Email = "ajacobm@pm.me"
        }
    });
    
    // Include XML documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure OpenTelemetry
var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "AlexLee.Api";
var serviceVersion = builder.Configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["service.instance.id"] = Environment.MachineName
        }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.Filter = httpContext => 
            {
                // Filter out health checks and swagger from tracing to reduce noise
                var path = httpContext.Request.Path.Value?.ToLower() ?? "";
                return !path.Contains("/health") && 
                       !path.Contains("/swagger") && 
                       !path.Contains("/favicon.ico");
            };
            options.RecordException = true;
        })
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.SetDbStatementForStoredProcedure = true;
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddSqlClientInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.RecordException = true;
        })
        .AddConsoleExporter()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddMeter("AlexLee.Application")
        .AddConsoleExporter()
        .AddOtlpExporter());

// Add Azure Application Insights (if configured)
var appInsightsConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
        options.EnableAdaptiveSampling = true;
        options.EnableHeartbeat = true;
        options.EnableDebugLogger = builder.Environment.IsDevelopment();
    });
}

// Configure logging with database logger
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add custom database logging for Warning and Error levels
builder.Logging.AddDatabaseLogger(config =>
{
    config.MinimumLevel = LogLevel.Warning;
    config.BufferSize = 50;
    config.FlushInterval = TimeSpan.FromSeconds(30);
    config.IncludeScopes = true;
});

// Add Application and Infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "https://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add health checks with SQL Server check
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AlexLeeDbContext>("database");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alex Lee API v1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

// Add telemetry middleware to capture request metrics
app.UseMiddleware<TelemetryMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Initialize database with SQL Server Express and SQL scripts
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AlexLeeDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting database initialization...");
        
        // Ensure database exists (this will create it if it doesn't exist)
        var created = await context.Database.EnsureCreatedAsync();
        
        if (created)
        {
            logger.LogInformation("Database created successfully.");
        }
        else
        {
            logger.LogInformation("Database already exists.");
        }
        
        // Initialize database using SQL scripts (replaces hardcoded seed data)
        await context.InitializeDatabaseAsync();
        
        logger.LogInformation("Database initialization completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
        
        // In development, we might want to continue anyway
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
        
        logger.LogWarning("Continuing startup despite database initialization error (Development environment).");
    }
}

app.Run();
