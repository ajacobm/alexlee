namespace AlexLee.Domain.Entities;

/// <summary>
/// Immutable record representing a purchase detail item
/// Maps to the PurchaseDetailItem table from the SQL script
/// </summary>
public record PurchaseDetailItem
{
    public long PurchaseDetailItemAutoId { get; init; }
    public string PurchaseOrderNumber { get; init; } = string.Empty;
    public int ItemNumber { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public string? ItemDescription { get; init; }
    public decimal PurchasePrice { get; init; }
    public int PurchaseQuantity { get; init; }
    public string LastModifiedByUser { get; init; } = string.Empty;
    public DateTime LastModifiedDateTime { get; init; }
    
    // Computed property for line number (will be set by query)
    public int? LineNumber { get; init; }
}

/// <summary>
/// Value object for purchase detail creation/update
/// </summary>
public record CreatePurchaseDetailItem
{
    public string PurchaseOrderNumber { get; init; } = string.Empty;
    public int ItemNumber { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public string? ItemDescription { get; init; }
    public decimal PurchasePrice { get; init; }
    public int PurchaseQuantity { get; init; }
    public string LastModifiedByUser { get; init; } = string.Empty;
}

/// <summary>
/// Value object for purchase detail updates
/// </summary>
public record UpdatePurchaseDetailItem
{
    public long PurchaseDetailItemAutoId { get; init; }
    public string PurchaseOrderNumber { get; init; } = string.Empty;
    public int ItemNumber { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public string? ItemDescription { get; init; }
    public decimal PurchasePrice { get; init; }
    public int PurchaseQuantity { get; init; }
    public string LastModifiedByUser { get; init; } = string.Empty;
}

/// <summary>
/// Filter criteria for purchase detail queries
/// </summary>
public record PurchaseDetailFilter
{
    public string? PurchaseOrderNumber { get; init; }
    public int? ItemNumber { get; init; }
    public string? ItemName { get; init; }
    public string? ItemDescription { get; init; }
}