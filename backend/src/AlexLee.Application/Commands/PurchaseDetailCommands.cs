using MediatR;
using AlexLee.Domain.Entities;

namespace AlexLee.Application.Commands;

/// <summary>
/// Command to create a new purchase detail item
/// </summary>
public record CreatePurchaseDetailCommand : IRequest<PurchaseDetailItem>
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
/// Command to update an existing purchase detail item
/// </summary>
public record UpdatePurchaseDetailCommand : IRequest<PurchaseDetailItem?>
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
/// Command to delete a purchase detail item
/// </summary>
public record DeletePurchaseDetailCommand : IRequest<bool>
{
    public long PurchaseDetailItemAutoId { get; init; }
}