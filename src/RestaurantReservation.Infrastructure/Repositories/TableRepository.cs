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

public class TableRepository : GenericRepository<Table>, ITableRepository
{
    public TableRepository(RestaurantDbContext context) : base(context)
    {
    }

    public async Task<Table?> GetByTableNumberAsync(string tableNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.TableNumber.ToLower() == tableNumber.ToLower());
    }

    public async Task<IEnumerable<Table>> GetAvailableTablesAsync(DateTime date, int partySize)
    {
        var startTime = date;
        var endTime = date.AddHours(3); // Default 3 hour window

        // Get tables that can accommodate the party size and are active
        var suitableTables = await _dbSet
            .Where(t => t.IsActive &&
                       t.Capacity >= partySize &&
                       (t.MinimumCapacity == null || partySize >= t.MinimumCapacity))
            .Include(t => t.Reservations)
            .ToListAsync();

        // Filter out tables that have conflicting reservations
        var availableTables = suitableTables.Where(table =>
            !table.Reservations.Any(r =>
                r.ReservationDate < endTime &&
                r.ReservationDate.AddMinutes(r.Duration) > startTime &&
                r.Status != "Cancelled" &&
                r.Status != "Completed"))
            .ToList();

        return availableTables;
    }

    public async Task<IEnumerable<Table>> GetTablesByLocationAsync(string location)
    {
        return await _dbSet
            .Where(t => t.Location != null && t.Location.ToLower().Contains(location.ToLower()))
            .ToListAsync();
    }

    public async Task<IEnumerable<Table>> GetTablesByCapacityAsync(int minCapacity, int? maxCapacity = null)
    {
        var query = _dbSet.Where(t => t.Capacity >= minCapacity && t.IsActive);

        if (maxCapacity.HasValue)
        {
            query = query.Where(t => t.Capacity <= maxCapacity.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<(IEnumerable<Table> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        bool? isAvailable = null,
        string? location = null)
    {
        var query = _dbSet.AsQueryable();

        if (isAvailable.HasValue)
        {
            query = query.Where(t => t.IsActive == isAvailable.Value);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(t => t.Location != null && t.Location.ToLower().Contains(location.ToLower()));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.TableNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> IsTableAvailableAsync(int tableId, DateTime date, int duration)
    {
        var table = await _dbSet
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.TableId == tableId);

        if (table == null || !table.IsActive)
            return false;

        var endTime = date.AddMinutes(duration);

        // Check if there are any conflicting reservations
        var hasConflict = table.Reservations.Any(r =>
            r.ReservationDate < endTime &&
            r.ReservationDate.AddMinutes(r.Duration) > date &&
            r.Status != "Cancelled" &&
            r.Status != "Completed");

        return !hasConflict;
    }

    public override async Task<Table?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.TableId == id);
    }
}
