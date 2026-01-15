// File: src/RestaurantReservation.API/Controllers/ReservationsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.DTOs.Common;
using RestaurantReservation.Application.Interfaces;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize] // Require authentication
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        IReservationService reservationService,
        ILogger<ReservationsController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all reservations with pagination and filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReservationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ReservationDto>>>> GetAllReservations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? date = null,
        [FromQuery] string? status = null,
        [FromQuery] int? customerId = null)
    {
        try
        {
            var result = await _reservationService.GetAllReservationsAsync(
                pageNumber, pageSize, date, status, customerId);
            return Ok(ApiResponse<PagedResult<ReservationDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservations");
            return StatusCode(500, ApiResponse<PagedResult<ReservationDto>>.ErrorResponse(
                "An error occurred while retrieving reservations"));
        }
    }

    /// <summary>
    /// Get a reservation by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> GetReservationById(int id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);

            if (reservation == null)
            {
                return NotFound(ApiResponse<ReservationDto>.ErrorResponse(
                    $"Reservation with ID {id} not found"));
            }

            return Ok(ApiResponse<ReservationDto>.SuccessResponse(reservation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservation {ReservationId}", id);
            return StatusCode(500, ApiResponse<ReservationDto>.ErrorResponse(
                "An error occurred while retrieving the reservation"));
        }
    }

    /// <summary>
    /// Get a reservation by confirmation code
    /// </summary>
    [HttpGet("confirmation/{confirmationCode}")]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> GetReservationByConfirmationCode(string confirmationCode)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByConfirmationCodeAsync(confirmationCode);

            if (reservation == null)
            {
                return NotFound(ApiResponse<ReservationDto>.ErrorResponse(
                    $"Reservation with confirmation code '{confirmationCode}' not found"));
            }

            return Ok(ApiResponse<ReservationDto>.SuccessResponse(reservation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservation by confirmation code");
            return StatusCode(500, ApiResponse<ReservationDto>.ErrorResponse(
                "An error occurred while retrieving the reservation"));
        }
    }

    /// <summary>
    /// Get all reservations for a customer
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReservationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReservationDto>>>> GetCustomerReservations(int customerId)
    {
        try
        {
            var reservations = await _reservationService.GetCustomerReservationsAsync(customerId);
            return Ok(ApiResponse<IEnumerable<ReservationDto>>.SuccessResponse(reservations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer reservations");
            return StatusCode(500, ApiResponse<IEnumerable<ReservationDto>>.ErrorResponse(
                "An error occurred while retrieving customer reservations"));
        }
    }

    /// <summary>
    /// Check availability for a date and party size
    /// </summary>
    [HttpGet("availability")]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityCheckDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AvailabilityCheckDto>>> CheckAvailability(
        [FromQuery] DateTime date,
        [FromQuery] int partySize)
    {
        try
        {
            var availability = await _reservationService.CheckAvailabilityAsync(date, partySize);
            return Ok(ApiResponse<AvailabilityCheckDto>.SuccessResponse(availability));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability");
            return StatusCode(500, ApiResponse<AvailabilityCheckDto>.ErrorResponse(
                "An error occurred while checking availability"));
        }
    }

    /// <summary>
    /// Create a new reservation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReservationConfirmationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReservationConfirmationDto>>> CreateReservation(
        [FromBody] CreateReservationDto createDto)
    {
        try
        {
            var confirmation = await _reservationService.CreateReservationAsync(createDto);

            return CreatedAtAction(
                nameof(GetReservationById),
                new { id = confirmation.ReservationId },
                ApiResponse<ReservationConfirmationDto>.SuccessResponse(
                    confirmation,
                    "Reservation created successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ReservationConfirmationDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ReservationConfirmationDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation");
            return StatusCode(500, ApiResponse<ReservationConfirmationDto>.ErrorResponse(
                "An error occurred while creating the reservation"));
        }
    }

    /// <summary>
    /// Update an existing reservation
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> UpdateReservation(
        int id,
        [FromBody] UpdateReservationDto updateDto)
    {
        try
        {
            var reservation = await _reservationService.UpdateReservationAsync(id, updateDto);
            return Ok(ApiResponse<ReservationDto>.SuccessResponse(
                reservation,
                "Reservation updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ReservationDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ReservationDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reservation {ReservationId}", id);
            return StatusCode(500, ApiResponse<ReservationDto>.ErrorResponse(
                "An error occurred while updating the reservation"));
        }
    }

    /// <summary>
    /// Cancel a reservation
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> CancelReservation(
        int id,
        [FromQuery] string? reason = null)
    {
        try
        {
            var result = await _reservationService.CancelReservationAsync(id, reason);

            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    $"Reservation with ID {id} not found"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                new { reservationId = id, cancelled = true },
                "Reservation cancelled successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation {ReservationId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while cancelling the reservation"));
        }
    }

    /// <summary>
    /// Confirm a pending reservation
    /// </summary>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ConfirmReservation(int id)
    {
        try
        {
            var result = await _reservationService.ConfirmReservationAsync(id);

            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    $"Reservation with ID {id} not found"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                new { reservationId = id, confirmed = true },
                "Reservation confirmed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming reservation {ReservationId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while confirming the reservation"));
        }
    }

    /// <summary>
    /// Mark reservation as seated
    /// </summary>
    [HttpPost("{id}/seat")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> SeatReservation(int id)
    {
        try
        {
            var result = await _reservationService.SeatReservationAsync(id);

            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    $"Reservation with ID {id} not found"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                new { reservationId = id, seated = true },
                "Reservation marked as seated"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seating reservation {ReservationId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while seating the reservation"));
        }
    }

    /// <summary>
    /// Mark reservation as completed
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> CompleteReservation(int id)
    {
        try
        {
            var result = await _reservationService.CompleteReservationAsync(id);

            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    $"Reservation with ID {id} not found"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                new { reservationId = id, completed = true },
                "Reservation marked as completed"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing reservation {ReservationId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while completing the reservation"));
        }
    }

    /// <summary>
    /// Mark reservation as no-show
    /// </summary>
    [HttpPost("{id}/no-show")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsNoShow(int id)
    {
        try
        {
            var result = await _reservationService.MarkAsNoShowAsync(id);

            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    $"Reservation with ID {id} not found"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                new { reservationId = id, noShow = true },
                "Reservation marked as no-show"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking reservation as no-show {ReservationId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while marking the reservation as no-show"));
        }
    }
}