using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.DTOs.Common;

namespace RestaurantReservation.Application.Interfaces;

public interface ITableService
{
    Task<PagedResult<TableDto>> GetAllTablesAsync(int pageNumber, int pageSize, bool? isAvailable = null, string? location = null);
    Task<TableDto?> GetTableByIdAsync(int id);
    Task<TableDto?> GetTableByNumberAsync(string tableNumber);
    Task<IEnumerable<TableDto>> GetAvailableTablesAsync(DateTime date, int partySize);
    Task<TableDto> CreateTableAsync(CreateTableDto createDto);
    Task<TableDto> UpdateTableAsync(int id, UpdateTableDto updateDto);
    Task<bool> DeleteTableAsync(int id);
    Task<bool> TableExistsAsync(int id);
    Task<bool> IsTableAvailableAsync(int tableId, DateTime date, int duration);
    Task<TableScheduleDto?> GetTableScheduleAsync(int tableId, DateTime date);
}

