using Microsoft.EntityFrameworkCore;
using AlexLee.Domain.Entities;

namespace AlexLee.Infrastructure.Data;

/// <summary>
/// Main database context for the Alex Lee application
/// </summary>
public class AlexLeeDbContext : DbContext
{
    public AlexLeeDbContext(DbContextOptions<AlexLeeDbContext> options) : base(options)
    {
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
            
            // Configure properties
            entity.Property(e => e.PurchaseDetailItemAutoId)
                .HasColumnName("PurchaseDetailItemAutoId")
                .ValueGeneratedOnAdd(); // IDENTITY column
            
            entity.Property(e => e.PurchaseOrderNumber)
                .HasColumnName("PurchaseOrderNumber")
                .HasMaxLength(20)
                .IsRequired();
            
            entity.Property(e => e.ItemNumber)
                .HasColumnName("ItemNumber")
                .IsRequired();
                
            entity.Property(e => e.ItemName)
                .HasColumnName("ItemName")
                .HasMaxLength(50)
                .IsRequired();
                
            entity.Property(e => e.ItemDescription)
                .HasColumnName("ItemDescription")
                .HasMaxLength(250);
                
            entity.Property(e => e.PurchasePrice)
                .HasColumnName("PurchasePrice")
                .HasColumnType("DECIMAL(10,2)")
                .IsRequired();
                
            entity.Property(e => e.PurchaseQuantity)
                .HasColumnName("PurchaseQuantity")
                .IsRequired();
                
            entity.Property(e => e.LastModifiedByUser)
                .HasColumnName("LastModifiedByUser")
                .HasMaxLength(50)
                .IsRequired();
                
            entity.Property(e => e.LastModifiedDateTime)
                .HasColumnName("LastModifiedDateTime")
                .IsRequired();
                
            // LineNumber is computed and not stored in the database
            entity.Ignore(e => e.LineNumber);
        });
        
        // Seed data from the SQL script
        SeedData(modelBuilder);
    }
    
    /// <summary>
    /// Seeds the database with sample data from the SQL script
    /// </summary>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        var baseDateTime = new DateTime(2025, 1, 17, 0, 0, 0, DateTimeKind.Utc);
        
        modelBuilder.Entity<PurchaseDetailItem>().HasData(
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 1,
                PurchaseOrderNumber = "112334",
                ItemNumber = 4011,
                ItemName = "Banana",
                ItemDescription = "Box of Green Bananas",
                PurchasePrice = 112.19m,
                PurchaseQuantity = 50,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            },
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 2,
                PurchaseOrderNumber = "112334",
                ItemNumber = 4011,
                ItemName = "Banana",
                ItemDescription = "Box of Green Bananas",
                PurchasePrice = 112.19m,
                PurchaseQuantity = 50,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            },
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 3,
                PurchaseOrderNumber = "112334",
                ItemNumber = 4011,
                ItemName = "Banana",
                ItemDescription = "Box of Green Bananas",
                PurchasePrice = 112.19m,
                PurchaseQuantity = 50,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            },
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 4,
                PurchaseOrderNumber = "112335",
                ItemNumber = 4035,
                ItemName = "Gala Apple",
                ItemDescription = "Bag of gala apples",
                PurchasePrice = 212.33m,
                PurchaseQuantity = 125,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            },
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 5,
                PurchaseOrderNumber = "112334",
                ItemNumber = 4030,
                ItemName = "Kiwis",
                ItemDescription = "Bag of kiwis",
                PurchasePrice = 153.88m,
                PurchaseQuantity = 100,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            },
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 6,
                PurchaseOrderNumber = "112334",
                ItemNumber = 4035,
                ItemName = "Gala Apple",
                ItemDescription = "Bag of gala apples",
                PurchasePrice = 212.33m,
                PurchaseQuantity = 125,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            },
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 7,
                PurchaseOrderNumber = "112335",
                ItemNumber = 4030,
                ItemName = "Kiwis",
                ItemDescription = "Bag of kiwis",
                PurchasePrice = 109.88m,
                PurchaseQuantity = 76,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            },
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 8,
                PurchaseOrderNumber = "112335",
                ItemNumber = 4011,
                ItemName = "Banana",
                ItemDescription = "Box of Green Bananas",
                PurchasePrice = 67.45m,
                PurchaseQuantity = 26,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            },
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 9,
                PurchaseOrderNumber = "112335",
                ItemNumber = 4011,
                ItemName = "Banana",
                ItemDescription = "Box of Green Bananas",
                PurchasePrice = 67.45m,
                PurchaseQuantity = 26,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            },
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 10,
                PurchaseOrderNumber = "112335",
                ItemNumber = 4030,
                ItemName = "Kiwis",
                ItemDescription = "Bag of kiwis",
                PurchasePrice = 122.88m,
                PurchaseQuantity = 90,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            },
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 11,
                PurchaseOrderNumber = "112334",
                ItemNumber = 4030,
                ItemName = "Kiwis",
                ItemDescription = "Bag of kiwis",
                PurchasePrice = 153.88m,
                PurchaseQuantity = 100,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            },
            new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = 12,
                PurchaseOrderNumber = "112335",
                ItemNumber = 4035,
                ItemName = "Gala Apple",
                ItemDescription = "Bag of gala apples",
                PurchasePrice = 212.33m,
                PurchaseQuantity = 125,
                LastModifiedByUser = "system",
                LastModifiedDateTime = baseDateTime
            }
        );
    }
}