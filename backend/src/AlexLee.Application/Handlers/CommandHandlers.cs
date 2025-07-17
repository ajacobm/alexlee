using MediatR;
using Microsoft.EntityFrameworkCore;
using AlexLee.Application.Commands;
using AlexLee.Domain.Entities;
using AlexLee.Infrastructure.Data;

namespace AlexLee.Application.Handlers;

/// <summary>
/// Handler for CreatePurchaseDetailCommand
/// </summary>
public class CreatePurchaseDetailHandler : IRequestHandler<CreatePurchaseDetailCommand, PurchaseDetailItem>
{
    private readonly AlexLeeDbContext _context;
    
    public CreatePurchaseDetailHandler(AlexLeeDbContext context)
    {
        _context = context;
    }
    
    public async Task<PurchaseDetailItem> Handle(CreatePurchaseDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = new PurchaseDetailItem
        {
            PurchaseOrderNumber = request.PurchaseOrderNumber,
            ItemNumber = request.ItemNumber,
            ItemName = request.ItemName,
            ItemDescription = request.ItemDescription,
            PurchasePrice = request.PurchasePrice,
            PurchaseQuantity = request.PurchaseQuantity,
            LastModifiedByUser = request.LastModifiedByUser,
            LastModifiedDateTime = DateTime.UtcNow
        };
        
        _context.PurchaseDetailItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return entity;
    }
}

/// <summary>
/// Handler for UpdatePurchaseDetailCommand
/// </summary>
public class UpdatePurchaseDetailHandler : IRequestHandler<UpdatePurchaseDetailCommand, PurchaseDetailItem?>
{
    private readonly AlexLeeDbContext _context;
    
    public UpdatePurchaseDetailHandler(AlexLeeDbContext context)
    {
        _context = context;
    }
    
    public async Task<PurchaseDetailItem?> Handle(UpdatePurchaseDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PurchaseDetailItems
            .FirstOrDefaultAsync(x => x.PurchaseDetailItemAutoId == request.PurchaseDetailItemAutoId, cancellationToken);
            
        if (entity == null)
            return null;
        
        // Create new entity with updated values (immutable pattern)
        var updatedEntity = entity with
        {
            PurchaseOrderNumber = request.PurchaseOrderNumber,
            ItemNumber = request.ItemNumber,
            ItemName = request.ItemName,
            ItemDescription = request.ItemDescription,
            PurchasePrice = request.PurchasePrice,
            PurchaseQuantity = request.PurchaseQuantity,
            LastModifiedByUser = request.LastModifiedByUser,
            LastModifiedDateTime = DateTime.UtcNow
        };
        
        // Update in EF Core context
        _context.Entry(entity).CurrentValues.SetValues(updatedEntity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return updatedEntity;
    }
}

/// <summary>
/// Handler for DeletePurchaseDetailCommand
/// </summary>
public class DeletePurchaseDetailHandler : IRequestHandler<DeletePurchaseDetailCommand, bool>
{
    private readonly AlexLeeDbContext _context;
    
    public DeletePurchaseDetailHandler(AlexLeeDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> Handle(DeletePurchaseDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PurchaseDetailItems
            .FirstOrDefaultAsync(x => x.PurchaseDetailItemAutoId == request.PurchaseDetailItemAutoId, cancellationToken);
            
        if (entity == null)
            return false;
        
        _context.PurchaseDetailItems.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}