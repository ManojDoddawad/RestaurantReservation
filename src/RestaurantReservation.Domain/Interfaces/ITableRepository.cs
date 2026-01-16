using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Domain.Interfaces;

public interface ITableRepository : IGenericRepository<Table>
{
    Task<Table?> GetByTableNumberAsync(string tableNumber);
    Task<IEnumerable<Table>> GetAvailableTablesAsync(DateTime date, int partySize);
    Task<IEnumerable<Table>> GetTablesByLocationAsync(string location);
    Task<IEnumerable<Table>> GetTablesByCapacityAsync(int minCapacity, int? maxCapacity = null);
    Task<(IEnumerable<Table> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, bool? isAvailable = null, string? location = null);
    Task<bool> IsTableAvailableAsync(int tableId, DateTime date, int duration);
    Task<Table?> GetTableWithReservationsAsync(int tableId);

}