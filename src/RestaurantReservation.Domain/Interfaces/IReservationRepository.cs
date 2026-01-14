using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Domain.Interfaces;

public interface IReservationRepository : IGenericRepository<Reservation>
{
    Task<Reservation?> GetByConfirmationCodeAsync(string confirmationCode);
    Task<IEnumerable<Reservation>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Reservation>> GetByDateAsync(DateTime date);
    Task<IEnumerable<Reservation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Reservation>> GetByStatusAsync(string status);
    Task<(IEnumerable<Reservation> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        DateTime? date = null,
        string? status = null,
        int? customerId = null);
    Task<IEnumerable<Reservation>> GetUpcomingReservationsAsync(int hours = 24);
    Task<bool> HasConflictingReservationAsync(int tableId, DateTime date, int duration, int? excludeReservationId = null);
}
