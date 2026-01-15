// File: src/RestaurantReservation.API/Controllers/CustomersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.DTOs.Customer;
using RestaurantReservation.Application.DTOs.Common;
using RestaurantReservation.Application.Interfaces;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize] // Require authentication for all endpoints
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers with pagination and optional search (Admin/Staff only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerDto>>>> GetAllCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var result = await _customerService.GetAllCustomersAsync(pageNumber, pageSize, searchTerm);
            return Ok(ApiResponse<PagedResult<CustomerDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers");
            return StatusCode(500, ApiResponse<PagedResult<CustomerDto>>.ErrorResponse(
                "An error occurred while retrieving customers"));
        }
    }

    /// <summary>
    /// Get a customer by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerById(int id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                return NotFound(ApiResponse<CustomerDto>.ErrorResponse(
                    $"Customer with ID {id} not found"));
            }

            return Ok(ApiResponse<CustomerDto>.SuccessResponse(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {CustomerId}", id);
            return StatusCode(500, ApiResponse<CustomerDto>.ErrorResponse(
                "An error occurred while retrieving the customer"));
        }
    }

    /// <summary>
    /// Get a customer by email
    /// </summary>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerByEmail(string email)
    {
        try
        {
            var customer = await _customerService.GetCustomerByEmailAsync(email);

            if (customer == null)
            {
                return NotFound(ApiResponse<CustomerDto>.ErrorResponse(
                    $"Customer with email {email} not found"));
            }

            return Ok(ApiResponse<CustomerDto>.SuccessResponse(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by email {Email}", email);
            return StatusCode(500, ApiResponse<CustomerDto>.ErrorResponse(
                "An error occurred while retrieving the customer"));
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer(
        [FromBody] CreateCustomerDto createDto)
    {
        try
        {
            var customer = await _customerService.CreateCustomerAsync(createDto);

            return CreatedAtAction(
                nameof(GetCustomerById),
                new { id = customer.CustomerId },
                ApiResponse<CustomerDto>.SuccessResponse(customer, "Customer created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CustomerDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, ApiResponse<CustomerDto>.ErrorResponse(
                "An error occurred while creating the customer"));
        }
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateCustomer(
        int id,
        [FromBody] UpdateCustomerDto updateDto)
    {
        try
        {
            var customer = await _customerService.UpdateCustomerAsync(id, updateDto);
            return Ok(ApiResponse<CustomerDto>.SuccessResponse(customer, "Customer updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CustomerDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CustomerDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", id);
            return StatusCode(500, ApiResponse<CustomerDto>.ErrorResponse(
                "An error occurred while updating the customer"));
        }
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        try
        {
            var result = await _customerService.DeleteCustomerAsync(id);

            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    $"Customer with ID {id} not found"));
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while deleting the customer"));
        }
    }
}