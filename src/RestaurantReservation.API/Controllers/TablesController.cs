using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.DTOs.Common;
using RestaurantReservation.Application.Interfaces;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TablesController : ControllerBase
{
    private readonly ITableService _tableService;
    private readonly ILogger<TablesController> _logger;

    public TablesController(
        ITableService tableService,
        ILogger<TablesController> logger)
    {
        _tableService = tableService;
        _logger = logger;
    }

    /// <summary>
    /// Get all tables with pagination and optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TableDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<TableDto>>>> GetAllTables(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isAvailable = null,
        [FromQuery] string? location = null)
    {
        try
        {
            var result = await _tableService.GetAllTablesAsync(pageNumber, pageSize, isAvailable, location);
            return Ok(ApiResponse<PagedResult<TableDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tables");
            return StatusCode(500, ApiResponse<PagedResult<TableDto>>.ErrorResponse(
                "An error occurred while retrieving tables"));
        }
    }

    /// <summary>
    /// Get a table by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TableDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TableDto>>> GetTableById(int id)
    {
        try
        {
            var table = await _tableService.GetTableByIdAsync(id);

            if (table == null)
            {
                return NotFound(ApiResponse<TableDto>.ErrorResponse(
                    $"Table with ID {id} not found"));
            }

            return Ok(ApiResponse<TableDto>.SuccessResponse(table));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table {TableId}", id);
            return StatusCode(500, ApiResponse<TableDto>.ErrorResponse(
                "An error occurred while retrieving the table"));
        }
    }

    /// <summary>
    /// Get a table by table number
    /// </summary>
    [HttpGet("number/{tableNumber}")]
    [ProducesResponseType(typeof(ApiResponse<TableDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TableDto>>> GetTableByNumber(string tableNumber)
    {
        try
        {
            var table = await _tableService.GetTableByNumberAsync(tableNumber);

            if (table == null)
            {
                return NotFound(ApiResponse<TableDto>.ErrorResponse(
                    $"Table number '{tableNumber}' not found"));
            }

            return Ok(ApiResponse<TableDto>.SuccessResponse(table));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table by number {TableNumber}", tableNumber);
            return StatusCode(500, ApiResponse<TableDto>.ErrorResponse(
                "An error occurred while retrieving the table"));
        }
    }

    /// <summary>
    /// Get available tables for a specific date and party size
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TableDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TableDto>>>> GetAvailableTables(
        [FromQuery] DateTime date,
        [FromQuery] int partySize)
    {
        try
        {
            var tables = await _tableService.GetAvailableTablesAsync(date, partySize);
            return Ok(ApiResponse<IEnumerable<TableDto>>.SuccessResponse(
                tables,
                $"Found {tables.Count()} available tables"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available tables");
            return StatusCode(500, ApiResponse<IEnumerable<TableDto>>.ErrorResponse(
                "An error occurred while checking table availability"));
        }
    }

    /// <summary>
    /// Check if a specific table is available
    /// </summary>
    [HttpGet("{id}/availability")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> CheckTableAvailability(
        int id,
        [FromQuery] DateTime date,
        [FromQuery] int duration = 120)
    {
        try
        {
            var isAvailable = await _tableService.IsTableAvailableAsync(id, date, duration);
            return Ok(ApiResponse<bool>.SuccessResponse(
                isAvailable,
                isAvailable ? "Table is available" : "Table is not available for the specified time"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking table availability");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "An error occurred while checking table availability"));
        }
    }

    /// <summary>
    /// Get table schedule for a specific date
    /// </summary>
    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(ApiResponse<TableScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TableScheduleDto>>> GetTableSchedule(
        int id,
        [FromQuery] DateTime date)
    {
        try
        {
            var schedule = await _tableService.GetTableScheduleAsync(id, date);

            if (schedule == null)
            {
                return NotFound(ApiResponse<TableScheduleDto>.ErrorResponse(
                    $"Table with ID {id} not found"));
            }

            return Ok(ApiResponse<TableScheduleDto>.SuccessResponse(schedule));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table schedule");
            return StatusCode(500, ApiResponse<TableScheduleDto>.ErrorResponse(
                "An error occurred while retrieving the table schedule"));
        }
    }

    /// <summary>
    /// Create a new table
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TableDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TableDto>>> CreateTable(
        [FromBody] CreateTableDto createDto)
    {
        try
        {
            var table = await _tableService.CreateTableAsync(createDto);

            return CreatedAtAction(
                nameof(GetTableById),
                new { id = table.TableId },
                ApiResponse<TableDto>.SuccessResponse(table, "Table created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TableDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating table");
            return StatusCode(500, ApiResponse<TableDto>.ErrorResponse(
                "An error occurred while creating the table"));
        }
    }

    /// <summary>
    /// Update an existing table
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TableDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TableDto>>> UpdateTable(
        int id,
        [FromBody] UpdateTableDto updateDto)
    {
        try
        {
            var table = await _tableService.UpdateTableAsync(id, updateDto);
            return Ok(ApiResponse<TableDto>.SuccessResponse(table, "Table updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TableDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TableDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table {TableId}", id);
            return StatusCode(500, ApiResponse<TableDto>.ErrorResponse(
                "An error occurred while updating the table"));
        }
    }

    /// <summary>
    /// Delete a table
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTable(int id)
    {
        try
        {
            var result = await _tableService.DeleteTableAsync(id);

            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    $"Table with ID {id} not found"));
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting table {TableId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while deleting the table"));
        }
    }
}