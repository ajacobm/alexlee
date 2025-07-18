using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using AlexLee.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AlexLee.Infrastructure.Data;

/// <summary>
/// Main database context for the Alex Lee application
/// </summary>
public class AlexLeeDbContext : DbContext
{
    private readonly ILogger<AlexLeeDbContext>? _logger;
    
    public AlexLeeDbContext(DbContextOptions<AlexLeeDbContext> options) : base(options)
    {
    }
    
    public AlexLeeDbContext(DbContextOptions<AlexLeeDbContext> options, ILogger<AlexLeeDbContext> logger) 
        : base(options)
    {
        _logger = logger;
    }
    
    public DbSet<PurchaseDetailItem> PurchaseDetailItems { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure PurchaseDetailItem entity
        modelBuilder.Entity<PurchaseDetailItem>(entity =>
        {
            // Table name matches the SQL script
            entity.ToTable("PurchaseDetailItem");
            
            // Primary key
            entity.HasKey(e => e.PurchaseDetailItemAutoId);
            
            // Configure properties to match SQL Server schema
            entity.Property(e => e.PurchaseDetailItemAutoId)
                .HasColumnName("PurchaseDetailItemAutoId")
                .ValueGeneratedOnAdd() // IDENTITY column
                .HasColumnType("bigint");
            
            entity.Property(e => e.PurchaseOrderNumber)
                .HasColumnName("PurchaseOrderNumber")
                .HasMaxLength(20)
                .HasColumnType("varchar(20)")
                .IsRequired();
            
            entity.Property(e => e.ItemNumber)
                .HasColumnName("ItemNumber")
                .HasColumnType("int")
                .IsRequired();
                
            entity.Property(e => e.ItemName)
                .HasColumnName("ItemName")
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
                
            entity.Property(e => e.ItemDescription)
                .HasColumnName("ItemDescription")
                .HasMaxLength(250)
                .HasColumnType("varchar(250)");
                
            entity.Property(e => e.PurchasePrice)
                .HasColumnName("PurchasePrice")
                .HasColumnType("decimal(10,2)")
                .IsRequired();
                
            entity.Property(e => e.PurchaseQuantity)
                .HasColumnName("PurchaseQuantity")
                .HasColumnType("int")
                .IsRequired();
                
            entity.Property(e => e.LastModifiedByUser)
                .HasColumnName("LastModifiedByUser")
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
                
            entity.Property(e => e.LastModifiedDateTime)
                .HasColumnName("LastModifiedDateTime")
                .HasColumnType("datetime")
                .IsRequired();
                
            // LineNumber is computed and not stored in the database
            entity.Ignore(e => e.LineNumber);
        });
        
        // No hardcoded seed data - data will come from SQL script
    }
    
    /// <summary>
    /// Executes the SQL initialization scripts if needed
    /// This replaces the hardcoded SeedData method
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        try
        {
            _logger?.LogInformation("Starting database initialization...");
            
            // Check if table exists and has data
            var tableExists = await CheckTableExistsAsync();
            if (!tableExists)
            {
                _logger?.LogInformation("Table does not exist or has no data. Running initialization...");
                await RunInitializationScriptAsync();
            }
            
            // Always ensure stored procedures are up to date
            await RunStoredProcedureScriptAsync();
            
            _logger?.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during database initialization");
            throw;
        }
    }
    
    /// <summary>
    /// Checks if the PurchaseDetailItem table exists and has data
    /// </summary>
    private async Task<bool> CheckTableExistsAsync()
    {
        try
        {
            var count = await PurchaseDetailItems.CountAsync();
            return count > 0;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Runs the database initialization script
    /// </summary>
    private async Task RunInitializationScriptAsync()
    {
        var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "init-database.sql");
        
        if (!File.Exists(scriptPath))
        {
            // Fallback to alternative paths
            var altPaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "scripts", "init-database.sql"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "scripts", "init-database.sql"),
                "/app/scripts/init-database.sql" // Docker path
            };
            
            foreach (var altPath in altPaths)
            {
                if (File.Exists(altPath))
                {
                    scriptPath = altPath;
                    break;
                }
            }
        }
        
        if (!File.Exists(scriptPath))
        {
            _logger?.LogWarning("Database initialization script not found at {ScriptPath}. Skipping initialization.", scriptPath);
            return;
        }
        
        var script = await File.ReadAllTextAsync(scriptPath);
        await ExecuteSqlScriptAsync(script, "Database initialization");
    }
    
    /// <summary>
    /// Runs the stored procedures script
    /// </summary>
    private async Task RunStoredProcedureScriptAsync()
    {
        var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "stored-procedures.sql");
        
        if (!File.Exists(scriptPath))
        {
            // Fallback to alternative paths
            var altPaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "scripts", "stored-procedures.sql"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "scripts", "stored-procedures.sql"),
                "/app/scripts/stored-procedures.sql" // Docker path
            };
            
            foreach (var altPath in altPaths)
            {
                if (File.Exists(altPath))
                {
                    scriptPath = altPath;
                    break;
                }
            }
        }
        
        if (!File.Exists(scriptPath))
        {
            _logger?.LogWarning("Stored procedures script not found at {ScriptPath}. Skipping stored procedures setup.", scriptPath);
            return;
        }
        
        var script = await File.ReadAllTextAsync(scriptPath);
        await ExecuteSqlScriptAsync(script, "Stored procedures setup");
    }
    
    /// <summary>
    /// Executes a SQL script by splitting it into batches (GO statements)
    /// </summary>
    private async Task ExecuteSqlScriptAsync(string script, string scriptDescription)
    {
        try
        {
            _logger?.LogInformation("Executing {ScriptDescription}...", scriptDescription);
            
            // Split script by GO statements
            var batches = script
                .Split(new[] { "\nGO", "\rGO", "\r\nGO" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(batch => !string.IsNullOrWhiteSpace(batch))
                .ToArray();
            
            foreach (var batch in batches)
            {
                var trimmedBatch = batch.Trim();
                if (string.IsNullOrWhiteSpace(trimmedBatch)) continue;
                
                try
                {
                    await Database.ExecuteSqlRawAsync(trimmedBatch);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error executing batch in {ScriptDescription}: {Batch}", 
                        scriptDescription, trimmedBatch.Length > 100 ? trimmedBatch[..100] + "..." : trimmedBatch);
                    // Continue with next batch rather than failing completely
                }
            }
            
            _logger?.LogInformation("{ScriptDescription} completed successfully.", scriptDescription);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error executing {ScriptDescription}", scriptDescription);
            throw;
        }
    }
    
    /// <summary>
    /// Calls the stored procedure to get purchase details with line numbers
    /// </summary>
    public async Task<List<PurchaseDetailItem>> GetPurchaseDetailsWithLineNumbersAsync()
    {
        try
        {
            var result = await PurchaseDetailItems
                .FromSqlRaw("EXEC dbo.GetPurchaseDetailsWithLineNumbers")
                .ToListAsync();
            
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error executing GetPurchaseDetailsWithLineNumbers stored procedure");
            throw;
        }
    }
    
    /// <summary>
    /// Calls the stored procedure to get duplicate purchase details
    /// </summary>
    public async Task<List<PurchaseDetailItem>> GetDuplicatePurchaseDetailsAsync()
    {
        try
        {
            var result = await PurchaseDetailItems
                .FromSqlRaw("EXEC dbo.GetDuplicatePurchaseDetails")
                .ToListAsync();
            
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error executing GetDuplicatePurchaseDetails stored procedure");
            throw;
        }
    }
}