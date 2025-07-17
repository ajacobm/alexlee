using MediatR;
using AlexLee.Domain.Entities;

namespace AlexLee.Application.Queries;

/// <summary>
/// Query to get all purchase detail items with optional filtering
/// </summary>
public record GetPurchaseDetailsQuery : IRequest<IEnumerable<PurchaseDetailItem>>
{
    public PurchaseDetailFilter? Filter { get; init; }
}

/// <summary>
/// Query to get purchase detail by ID
/// </summary>
public record GetPurchaseDetailByIdQuery : IRequest<PurchaseDetailItem?>
{
    public long PurchaseDetailItemAutoId { get; init; }
}

/// <summary>
/// Query to identify duplicate purchase detail items
/// As per SQL problem #5
/// </summary>
public record GetDuplicatePurchaseDetailsQuery : IRequest<IEnumerable<DuplicatePurchaseDetailGroup>>
{
}

/// <summary>
/// Result for duplicate detection query
/// </summary>
public record DuplicatePurchaseDetailGroup
{
    public string PurchaseOrderNumber { get; init; } = string.Empty;
    public int ItemNumber { get; init; }
    public decimal PurchasePrice { get; init; }
    public int PurchaseQuantity { get; init; }
    public int Count { get; init; }
    public IEnumerable<long> PurchaseDetailItemAutoIds { get; init; } = Enumerable.Empty<long>();
}