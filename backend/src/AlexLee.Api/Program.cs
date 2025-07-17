using AlexLee.Application;
using AlexLee.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Reflection;

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
        Description = "REST API for Purchase Detail Management System",
        Contact = new OpenApiContact
        {
            Name = "Alex Lee Developer Exercise",
            Email = "developer@alexlee.com"
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

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alex Lee API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AlexLee.Infrastructure.Data.AlexLeeDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    // Add sample data if database is empty
    if (!context.PurchaseDetailItems.Any())
    {
        var sampleData = new[]
        {
            new AlexLee.Domain.Entities.PurchaseDetailItem
            {
                PurchaseOrderNumber = "PO-2024-001",
                ItemNumber = 100,
                ItemName = "Wireless Keyboard",
                ItemDescription = "Ergonomic wireless keyboard with RGB backlighting",
                PurchasePrice = 89.99m,
                PurchaseQuantity = 5,
                LastModifiedByUser = "admin",
                LastModifiedDateTime = DateTime.UtcNow
            },
            new AlexLee.Domain.Entities.PurchaseDetailItem
            {
                PurchaseOrderNumber = "PO-2024-001",
                ItemNumber = 101,
                ItemName = "Wireless Mouse",
                ItemDescription = "Precision wireless mouse with customizable buttons",
                PurchasePrice = 45.99m,
                PurchaseQuantity = 5,
                LastModifiedByUser = "admin",
                LastModifiedDateTime = DateTime.UtcNow
            },
            new AlexLee.Domain.Entities.PurchaseDetailItem
            {
                PurchaseOrderNumber = "PO-2024-002", 
                ItemNumber = 200,
                ItemName = "24-inch Monitor",
                ItemDescription = "4K UHD display with USB-C connectivity",
                PurchasePrice = 299.99m,
                PurchaseQuantity = 2,
                LastModifiedByUser = "manager",
                LastModifiedDateTime = DateTime.UtcNow
            },
            new AlexLee.Domain.Entities.PurchaseDetailItem
            {
                PurchaseOrderNumber = "PO-2024-003",
                ItemNumber = 300,
                ItemName = "Standing Desk",
                ItemDescription = "Height-adjustable electric standing desk",
                PurchasePrice = 599.99m,
                PurchaseQuantity = 1,
                LastModifiedByUser = "admin",
                LastModifiedDateTime = DateTime.UtcNow
            }
        };

        context.PurchaseDetailItems.AddRange(sampleData);
        await context.SaveChangesAsync();
    }
}

app.Run();
