using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Interfaces;
using RestaurantReservation.Infrastructure.Data;

namespace RestaurantReservation.Infrastructure.Repositories;

public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(RestaurantDbContext context) : base(context)
    {
    }

    public async Task<Reservation?> GetByConfirmationCodeAsync(string confirmationCode)
    {
        return await _dbSet
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.ConfirmationCode == confirmationCode);
    }

    public async Task<IEnumerable<Reservation>> GetByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Include(r => r.Table)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.ReservationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);

        return await _dbSet
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Where(r => r.ReservationDate >= startOfDay && r.ReservationDate < endOfDay)
            .OrderBy(r => r.ReservationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Where(r => r.ReservationDate >= startDate && r.ReservationDate <= endDate)
            .OrderBy(r => r.ReservationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.ReservationDate)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Reservation> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        DateTime? date = null,
        string? status = null,
        int? customerId = null)
    {
        var query = _dbSet
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .AsQueryable();

        if (date.HasValue)
        {
            var startOfDay = date.Value.Date;
            var endOfDay = date.Value.Date.AddDays(1);
            query = query.Where(r => r.ReservationDate >= startOfDay && r.ReservationDate < endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (customerId.HasValue)
        {
            query = query.Where(r => r.CustomerId == customerId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Reservation>> GetUpcomingReservationsAsync(int hours = 24)
    {
        var now = DateTime.Now;
        var futureTime = now.AddHours(hours);

        return await _dbSet
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Where(r => r.ReservationDate >= now &&
                       r.ReservationDate <= futureTime &&
                       r.Status == "Confirmed")
            .OrderBy(r => r.ReservationDate)
            .ToListAsync();
    }

    public async Task<bool> HasConflictingReservationAsync(
        int tableId,
        DateTime date,
        int duration,
        int? excludeReservationId = null)
    {
        var endTime = date.AddMinutes(duration);

        var query = _dbSet.Where(r =>
            r.TableId == tableId &&
            r.Status != "Cancelled" &&
            r.Status != "Completed" &&
            r.ReservationDate < endTime &&
            r.ReservationDate.AddMinutes(r.Duration) > date);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.ReservationId != excludeReservationId.Value);
        }

        return await query.AnyAsync();
    }

    public override async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.ReservationLogs)
            .FirstOrDefaultAsync(r => r.ReservationId == id);
    }
}
