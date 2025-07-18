using Microsoft.AspNetCore.Mvc;
using MediatR;
using AlexLee.Application.Commands;
using AlexLee.Application.Queries;
using AlexLee.Domain.Entities;
using AlexLee.Infrastructure.Data;

namespace AlexLee.Api.Controllers;

/// <summary>
/// API Controller for managing Purchase Detail items
/// Implements full CRUD operations and SQL Server stored procedures for Alex Lee Exercise
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PurchaseDetailsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PurchaseDetailsController> _logger;
    private readonly AlexLeeDbContext _dbContext;

    public PurchaseDetailsController(IMediator mediator, ILogger<PurchaseDetailsController> logger, AlexLeeDbContext dbContext)
    {
        _mediator = mediator;
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get all purchase detail items with optional filtering
    /// </summary>
    /// <param name="purchaseOrderNumber">Filter by purchase order number</param>
    /// <param name="itemNumber">Filter by item number</param>
    /// <param name="itemName">Filter by item name</param>
    /// <param name="itemDescription">Filter by item description</param>
    /// <returns>Collection of purchase detail items</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PurchaseDetailItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PurchaseDetailItem>>> GetPurchaseDetails(
        [FromQuery] string? purchaseOrderNumber = null,
        [FromQuery] int? itemNumber = null,
        [FromQuery] string? itemName = null,
        [FromQuery] string? itemDescription = null)
    {
        try
        {
            var filter = new PurchaseDetailFilter
            {
                PurchaseOrderNumber = purchaseOrderNumber,
                ItemNumber = itemNumber,
                ItemName = itemName,
                ItemDescription = itemDescription
            };

            var query = new GetPurchaseDetailsQuery { Filter = filter };
            var result = await _mediator.Send(query);

            _logger.LogInformation("Retrieved {Count} purchase details with filter: {Filter}", 
                result.Count(), filter);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase details with filter: {@Filter}", 
                new { purchaseOrderNumber, itemNumber, itemName, itemDescription });
            return BadRequest("Error retrieving purchase details");
        }
    }

    /// <summary>
    /// SQL Exercise Question #6: Get purchase detail records with line numbers using stored procedure
    /// Implements line numbering per item per purchase order from Question #4
    /// </summary>
    /// <returns>Purchase detail items with computed line numbers</returns>
    [HttpGet("with-line-numbers")]
    [ProducesResponseType(typeof(IEnumerable<PurchaseDetailItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PurchaseDetailItem>>> GetPurchaseDetailsWithLineNumbers()
    {
        try
        {
            var result = await _dbContext.GetPurchaseDetailsWithLineNumbersAsync();

            _logger.LogInformation("Retrieved {Count} purchase details with line numbers using stored procedure", 
                result.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase details with line numbers");
            return BadRequest("Error retrieving purchase details with line numbers");
        }
    }

    /// <summary>
    /// SQL Exercise Question #5: Get duplicate purchase detail records using stored procedure
    /// Identifies records with same purchase order number, item number, price, and quantity
    /// </summary>
    /// <returns>Duplicate purchase detail items</returns>
    [HttpGet("duplicates")]
    [ProducesResponseType(typeof(IEnumerable<PurchaseDetailItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PurchaseDetailItem>>> GetDuplicatePurchaseDetails()
    {
        try
        {
            var result = await _dbContext.GetDuplicatePurchaseDetailsAsync();

            _logger.LogInformation("Retrieved {Count} duplicate purchase details using stored procedure", 
                result.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving duplicate purchase details");
            return BadRequest("Error retrieving duplicate purchase details");
        }
    }

    /// <summary>
    /// Get a specific purchase detail item by ID
    /// </summary>
    /// <param name="id">Purchase detail item auto ID</param>
    /// <returns>Purchase detail item if found</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(PurchaseDetailItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PurchaseDetailItem>> GetPurchaseDetail(long id)
    {
        try
        {
            var query = new GetPurchaseDetailByIdQuery { PurchaseDetailItemAutoId = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Purchase detail with ID {Id} not found", id);
                return NotFound($"Purchase detail with ID {id} not found");
            }

            _logger.LogInformation("Retrieved purchase detail {Id}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase detail {Id}", id);
            return BadRequest("Error retrieving purchase detail");
        }
    }

    /// <summary>
    /// Create a new purchase detail item
    /// </summary>
    /// <param name="command">Purchase detail creation data</param>
    /// <returns>Created purchase detail item</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseDetailItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PurchaseDetailItem>> CreatePurchaseDetail([FromBody] CreatePurchaseDetailCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(command);

            _logger.LogInformation("Created purchase detail {Id} for order {OrderNumber}", 
                result.PurchaseDetailItemAutoId, result.PurchaseOrderNumber);

            return CreatedAtAction(
                nameof(GetPurchaseDetail),
                new { id = result.PurchaseDetailItemAutoId },
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase detail: {@Command}", command);
            return BadRequest("Error creating purchase detail");
        }
    }

    /// <summary>
    /// Update an existing purchase detail item
    /// </summary>
    /// <param name="id">Purchase detail item auto ID</param>
    /// <param name="command">Updated purchase detail data</param>
    /// <returns>Updated purchase detail item</returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(PurchaseDetailItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PurchaseDetailItem>> UpdatePurchaseDetail(long id, [FromBody] UpdatePurchaseDetailCommand command)
    {
        try
        {
            if (id != command.PurchaseDetailItemAutoId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(command);

            if (result == null)
            {
                _logger.LogWarning("Purchase detail with ID {Id} not found for update", id);
                return NotFound($"Purchase detail with ID {id} not found");
            }

            _logger.LogInformation("Updated purchase detail {Id}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating purchase detail {Id}: {@Command}", id, command);
            return BadRequest("Error updating purchase detail");
        }
    }

    /// <summary>
    /// Delete a purchase detail item
    /// </summary>
    /// <param name="id">Purchase detail item auto ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeletePurchaseDetail(long id)
    {
        try
        {
            var command = new DeletePurchaseDetailCommand { PurchaseDetailItemAutoId = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning("Purchase detail with ID {Id} not found for deletion", id);
                return NotFound($"Purchase detail with ID {id} not found");
            }

            _logger.LogInformation("Deleted purchase detail {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting purchase detail {Id}", id);
            return BadRequest("Error deleting purchase detail");
        }
    }

    /// <summary>
    /// Get summary information about the purchase detail data
    /// </summary>
    /// <returns>Summary statistics</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(PurchaseDetailSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PurchaseDetailSummaryResponse>> GetPurchaseDetailSummary()
    {
        try
        {
            var allItems = await _mediator.Send(new GetPurchaseDetailsQuery { Filter = new PurchaseDetailFilter() });
            var duplicates = await _dbContext.GetDuplicatePurchaseDetailsAsync();
            var withLineNumbers = await _dbContext.GetPurchaseDetailsWithLineNumbersAsync();

            var summary = new PurchaseDetailSummaryResponse
            {
                TotalRecords = allItems.Count(),
                DuplicateRecords = duplicates.Count(),
                UniquePurchaseOrders = allItems.Select(x => x.PurchaseOrderNumber).Distinct().Count(),
                UniqueItems = allItems.Select(x => x.ItemNumber).Distinct().Count(),
                TotalValue = allItems.Sum(x => x.PurchasePrice * x.PurchaseQuantity),
                StoredProceduresAvailable = new List<string>
                {
                    "GetPurchaseDetailsWithLineNumbers (Question #6)",
                    "GetDuplicatePurchaseDetails (Question #5)"
                },
                DatabaseType = "SQL Server Express",
                DataSource = "SQLExerciseScript.sql"
            };

            _logger.LogInformation("Generated purchase detail summary: {TotalRecords} total, {Duplicates} duplicates, {Orders} orders", 
                summary.TotalRecords, summary.DuplicateRecords, summary.UniquePurchaseOrders);

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating purchase detail summary");
            return BadRequest("Error generating purchase detail summary");
        }
    }
}

/// <summary>
/// Response model for purchase detail summary
/// </summary>
public record PurchaseDetailSummaryResponse
{
    public int TotalRecords { get; init; }
    public int DuplicateRecords { get; init; }
    public int UniquePurchaseOrders { get; init; }
    public int UniqueItems { get; init; }
    public decimal TotalValue { get; init; }
    public List<string> StoredProceduresAvailable { get; init; } = new();
    public string DatabaseType { get; init; } = string.Empty;
    public string DataSource { get; init; } = string.Empty;
}