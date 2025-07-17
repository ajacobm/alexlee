using MediatR;
using Microsoft.EntityFrameworkCore;
using AlexLee.Application.Queries;
using AlexLee.Domain.Entities;
using AlexLee.Infrastructure.Data;

namespace AlexLee.Application.Handlers;

/// <summary>
/// Handler for GetPurchaseDetailsQuery
/// </summary>
public class GetPurchaseDetailsHandler : IRequestHandler<GetPurchaseDetailsQuery, IEnumerable<PurchaseDetailItem>>
{
    private readonly AlexLeeDbContext _context;
    
    public GetPurchaseDetailsHandler(AlexLeeDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<PurchaseDetailItem>> Handle(GetPurchaseDetailsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PurchaseDetailItems.AsQueryable();
        
        // Apply filters if provided
        if (request.Filter != null)
        {
            if (!string.IsNullOrWhiteSpace(request.Filter.PurchaseOrderNumber))
            {
                query = query.Where(x => x.PurchaseOrderNumber.Contains(request.Filter.PurchaseOrderNumber));
            }
            
            if (request.Filter.ItemNumber.HasValue)
            {
                query = query.Where(x => x.ItemNumber == request.Filter.ItemNumber.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(request.Filter.ItemName))
            {
                query = query.Where(x => x.ItemName.Contains(request.Filter.ItemName));
            }
            
            if (!string.IsNullOrWhiteSpace(request.Filter.ItemDescription))
            {
                query = query.Where(x => x.ItemDescription != null && x.ItemDescription.Contains(request.Filter.ItemDescription));
            }
        }
        
        // Add line numbers using ROW_NUMBER() OVER (PARTITION BY PurchaseOrderNumber ORDER BY PurchaseDetailItemAutoId)
        // This solves SQL Problem #4
        var result = await query
            .Select(x => new PurchaseDetailItem
            {
                PurchaseDetailItemAutoId = x.PurchaseDetailItemAutoId,
                PurchaseOrderNumber = x.PurchaseOrderNumber,
                ItemNumber = x.ItemNumber,
                ItemName = x.ItemName,
                ItemDescription = x.ItemDescription,
                PurchasePrice = x.PurchasePrice,
                PurchaseQuantity = x.PurchaseQuantity,
                LastModifiedByUser = x.LastModifiedByUser,
                LastModifiedDateTime = x.LastModifiedDateTime,
                LineNumber = _context.PurchaseDetailItems
                    .Where(p => p.PurchaseOrderNumber == x.PurchaseOrderNumber && p.PurchaseDetailItemAutoId <= x.PurchaseDetailItemAutoId)
                    .Count()
            })
            .OrderBy(x => x.PurchaseOrderNumber)
            .ThenBy(x => x.PurchaseDetailItemAutoId)
            .ToListAsync(cancellationToken);
            
        return result;
    }
}

/// <summary>
/// Handler for GetPurchaseDetailByIdQuery
/// </summary>
public class GetPurchaseDetailByIdHandler : IRequestHandler<GetPurchaseDetailByIdQuery, PurchaseDetailItem?>
{
    private readonly AlexLeeDbContext _context;
    
    public GetPurchaseDetailByIdHandler(AlexLeeDbContext context)
    {
        _context = context;
    }
    
    public async Task<PurchaseDetailItem?> Handle(GetPurchaseDetailByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.PurchaseDetailItems
            .FirstOrDefaultAsync(x => x.PurchaseDetailItemAutoId == request.PurchaseDetailItemAutoId, cancellationToken);
    }
}

/// <summary>
/// Handler for GetDuplicatePurchaseDetailsQuery
/// Solves SQL Problem #5
/// </summary>
public class GetDuplicatePurchaseDetailsHandler : IRequestHandler<GetDuplicatePurchaseDetailsQuery, IEnumerable<DuplicatePurchaseDetailGroup>>
{
    private readonly AlexLeeDbContext _context;
    
    public GetDuplicatePurchaseDetailsHandler(AlexLeeDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<DuplicatePurchaseDetailGroup>> Handle(GetDuplicatePurchaseDetailsQuery request, CancellationToken cancellationToken)
    {
        // Group by the criteria and find duplicates
        var duplicates = await _context.PurchaseDetailItems
            .GroupBy(x => new { x.PurchaseOrderNumber, x.ItemNumber, x.PurchasePrice, x.PurchaseQuantity })
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicatePurchaseDetailGroup
            {
                PurchaseOrderNumber = g.Key.PurchaseOrderNumber,
                ItemNumber = g.Key.ItemNumber,
                PurchasePrice = g.Key.PurchasePrice,
                PurchaseQuantity = g.Key.PurchaseQuantity,
                Count = g.Count(),
                PurchaseDetailItemAutoIds = g.Select(x => x.PurchaseDetailItemAutoId)
            })
            .ToListAsync(cancellationToken);
            
        return duplicates;
    }
}