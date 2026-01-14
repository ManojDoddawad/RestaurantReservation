using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.DTOs.Common;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Interfaces;

namespace RestaurantReservation.Application.Services;

public class TableService : ITableService
{
    private readonly ITableRepository _tableRepository;

    public TableService(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task<PagedResult<TableDto>> GetAllTablesAsync(
        int pageNumber,
        int pageSize,
        bool? isAvailable = null,
        string? location = null)
    {
        var (items, totalCount) = await _tableRepository.GetPagedAsync(pageNumber, pageSize, isAvailable, location);

        var tableDtos = items.Select(t => MapToDto(t)).ToList();

        return new PagedResult<TableDto>
        {
            Data = tableDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount
        };
    }

    public async Task<TableDto?> GetTableByIdAsync(int id)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        return table != null ? MapToDto(table) : null;
    }

    public async Task<TableDto?> GetTableByNumberAsync(string tableNumber)
    {
        var table = await _tableRepository.GetByTableNumberAsync(tableNumber);
        return table != null ? MapToDto(table) : null;
    }

    public async Task<IEnumerable<TableDto>> GetAvailableTablesAsync(DateTime date, int partySize)
    {
        var tables = await _tableRepository.GetAvailableTablesAsync(date, partySize);
        return tables.Select(t => MapToDto(t)).ToList();
    }

    public async Task<TableDto> CreateTableAsync(CreateTableDto createDto)
    {
        // Check if table number already exists
        var existingTable = await _tableRepository.GetByTableNumberAsync(createDto.TableNumber);
        if (existingTable != null)
        {
            throw new InvalidOperationException($"A table with number '{createDto.TableNumber}' already exists.");
        }

        // Validate capacity ranges
        if (createDto.MinimumCapacity.HasValue && createDto.MinimumCapacity > createDto.Capacity)
        {
            throw new InvalidOperationException("Minimum capacity cannot be greater than capacity.");
        }

        if (createDto.MaximumCapacity.HasValue && createDto.MaximumCapacity < createDto.Capacity)
        {
            throw new InvalidOperationException("Maximum capacity cannot be less than capacity.");
        }

        var table = new Table
        {
            TableNumber = createDto.TableNumber,
            Capacity = createDto.Capacity,
            Location = createDto.Location,
            MinimumCapacity = createDto.MinimumCapacity,
            MaximumCapacity = createDto.MaximumCapacity,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var createdTable = await _tableRepository.AddAsync(table);
        return MapToDto(createdTable);
    }

    public async Task<TableDto> UpdateTableAsync(int id, UpdateTableDto updateDto)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table == null)
        {
            throw new KeyNotFoundException($"Table with ID {id} not found.");
        }

        // Check if table number is being changed and if new number already exists
        if (table.TableNumber != updateDto.TableNumber)
        {
            var existingTable = await _tableRepository.GetByTableNumberAsync(updateDto.TableNumber);
            if (existingTable != null)
            {
                throw new InvalidOperationException($"A table with number '{updateDto.TableNumber}' already exists.");
            }
        }

        // Validate capacity ranges
        if (updateDto.MinimumCapacity.HasValue && updateDto.MinimumCapacity > updateDto.Capacity)
        {
            throw new InvalidOperationException("Minimum capacity cannot be greater than capacity.");
        }

        if (updateDto.MaximumCapacity.HasValue && updateDto.MaximumCapacity < updateDto.Capacity)
        {
            throw new InvalidOperationException("Maximum capacity cannot be less than capacity.");
        }

        table.TableNumber = updateDto.TableNumber;
        table.Capacity = updateDto.Capacity;
        table.Location = updateDto.Location;
        table.IsActive = updateDto.IsActive;
        table.MinimumCapacity = updateDto.MinimumCapacity;
        table.MaximumCapacity = updateDto.MaximumCapacity;

        await _tableRepository.UpdateAsync(table);
        return MapToDto(table);
    }

    public async Task<bool> DeleteTableAsync(int id)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table == null)
        {
            return false;
        }

        // Check if table has active reservations
        var hasActiveReservations = table.Reservations?.Any(r =>
            r.Status != "Cancelled" &&
            r.Status != "Completed" &&
            r.ReservationDate >= DateTime.Now) ?? false;

        if (hasActiveReservations)
        {
            throw new InvalidOperationException("Cannot delete table with active reservations. Please cancel or complete existing reservations first.");
        }

        await _tableRepository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> TableExistsAsync(int id)
    {
        return await _tableRepository.ExistsAsync(id);
    }

    public async Task<bool> IsTableAvailableAsync(int tableId, DateTime date, int duration)
    {
        return await _tableRepository.IsTableAvailableAsync(tableId, date, duration);
    }

    public async Task<TableScheduleDto?> GetTableScheduleAsync(int tableId, DateTime date)
    {
        var table = await _tableRepository.GetByIdAsync(tableId);
        if (table == null)
        {
            return null;
        }

        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);

        var scheduleSlots = table.Reservations
            .Where(r => r.ReservationDate >= startOfDay &&
                       r.ReservationDate < endOfDay &&
                       r.Status != "Cancelled")
            .OrderBy(r => r.ReservationDate)
            .Select(r => new ScheduleSlotDto
            {
                StartTime = r.ReservationDate,
                EndTime = r.ReservationDate.AddMinutes(r.Duration),
                ReservationId = r.ReservationId,
                CustomerName = $"{r.Customer.FirstName} {r.Customer.LastName}",
                PartySize = r.PartySize,
                Status = r.Status
            })
            .ToList();

        return new TableScheduleDto
        {
            TableNumber = table.TableNumber,
            Date = date.Date,
            Schedule = scheduleSlots
        };
    }

    private TableDto MapToDto(Table table)
    {
        return new TableDto
        {
            TableId = table.TableId,
            TableNumber = table.TableNumber,
            Capacity = table.Capacity,
            Location = table.Location,
            IsActive = table.IsActive,
            MinimumCapacity = table.MinimumCapacity,
            MaximumCapacity = table.MaximumCapacity,
            CurrentStatus = table.IsActive ? "Available" : "Inactive",
            TotalReservations = table.Reservations?.Count ?? 0
        };
    }
}