using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.DTOs.Common;

namespace RestaurantReservation.Application.Interfaces;

public interface IReservationService
{
    Task<PagedResult<ReservationDto>> GetAllReservationsAsync(
        int pageNumber,
        int pageSize,
        DateTime? date = null,
        string? status = null,
        int? customerId = null);
    Task<ReservationDto?> GetReservationByIdAsync(int id);
    Task<ReservationDto?> GetReservationByConfirmationCodeAsync(string confirmationCode);
    Task<IEnumerable<ReservationDto>> GetCustomerReservationsAsync(int customerId);
    Task<ReservationConfirmationDto> CreateReservationAsync(CreateReservationDto createDto);
    Task<ReservationDto> UpdateReservationAsync(int id, UpdateReservationDto updateDto);
    Task<bool> CancelReservationAsync(int id, string? reason = null);
    Task<bool> ConfirmReservationAsync(int id);
    Task<bool> SeatReservationAsync(int id);
    Task<bool> CompleteReservationAsync(int id);
    Task<bool> MarkAsNoShowAsync(int id);
    Task<AvailabilityCheckDto> CheckAvailabilityAsync(DateTime date, int partySize);
}