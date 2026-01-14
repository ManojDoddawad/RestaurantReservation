using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.DTOs.Common;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Interfaces;

namespace RestaurantReservation.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITableRepository _tableRepository;

    public ReservationService(
        IReservationRepository reservationRepository,
        ICustomerRepository customerRepository,
        ITableRepository tableRepository)
    {
        _reservationRepository = reservationRepository;
        _customerRepository = customerRepository;
        _tableRepository = tableRepository;
    }

    public async Task<PagedResult<ReservationDto>> GetAllReservationsAsync(
        int pageNumber,
        int pageSize,
        DateTime? date = null,
        string? status = null,
        int? customerId = null)
    {
        var (items, totalCount) = await _reservationRepository.GetPagedAsync(
            pageNumber, pageSize, date, status, customerId);

        var reservationDtos = items.Select(r => MapToDto(r)).ToList();

        return new PagedResult<ReservationDto>
        {
            Data = reservationDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount
        };
    }

    public async Task<ReservationDto?> GetReservationByIdAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        return reservation != null ? MapToDto(reservation) : null;
    }

    public async Task<ReservationDto?> GetReservationByConfirmationCodeAsync(string confirmationCode)
    {
        var reservation = await _reservationRepository.GetByConfirmationCodeAsync(confirmationCode);
        return reservation != null ? MapToDto(reservation) : null;
    }

    public async Task<IEnumerable<ReservationDto>> GetCustomerReservationsAsync(int customerId)
    {
        var reservations = await _reservationRepository.GetByCustomerIdAsync(customerId);
        return reservations.Select(r => MapToDto(r)).ToList();
    }

    public async Task<ReservationConfirmationDto> CreateReservationAsync(CreateReservationDto createDto)
    {
        // Validate customer exists
        var customer = await _customerRepository.GetByIdAsync(createDto.CustomerId);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {createDto.CustomerId} not found.");
        }

        // Check if customer is blacklisted
        if (customer.IsBlacklisted)
        {
            throw new InvalidOperationException("This customer is blacklisted and cannot make reservations.");
        }

        // Validate reservation date is in the future
        if (createDto.ReservationDate <= DateTime.Now)
        {
            throw new InvalidOperationException("Reservation date must be in the future.");
        }

        // Validate reservation is not too far in advance (90 days)
        if (createDto.ReservationDate > DateTime.Now.AddDays(90))
        {
            throw new InvalidOperationException("Reservations can only be made up to 90 days in advance.");
        }

        // Validate party size
        if (createDto.PartySize < 1 || createDto.PartySize > 12)
        {
            throw new InvalidOperationException("Party size must be between 1 and 12.");
        }

        // Find available table
        Table? assignedTable = null;

        if (createDto.PreferredTableId.HasValue)
        {
            // Check if preferred table exists and is available
            var preferredTable = await _tableRepository.GetByIdAsync(createDto.PreferredTableId.Value);
            if (preferredTable != null && preferredTable.IsActive)
            {
                var isAvailable = await _tableRepository.IsTableAvailableAsync(
                    createDto.PreferredTableId.Value,
                    createDto.ReservationDate,
                    createDto.Duration);

                if (isAvailable && preferredTable.Capacity >= createDto.PartySize)
                {
                    assignedTable = preferredTable;
                }
            }
        }

        // If no preferred table or preferred table not available, find best available
        if (assignedTable == null)
        {
            var availableTables = await _tableRepository.GetAvailableTablesAsync(
                createDto.ReservationDate,
                createDto.PartySize);

            // Filter by preferred location if specified
            if (!string.IsNullOrWhiteSpace(createDto.PreferredTableLocation))
            {
                var locationTables = availableTables
                    .Where(t => t.Location != null &&
                               t.Location.ToLower().Contains(createDto.PreferredTableLocation.ToLower()))
                    .ToList();

                if (locationTables.Any())
                {
                    assignedTable = locationTables.OrderBy(t => t.Capacity).First();
                }
            }

            // If still no table, get the smallest suitable table
            if (assignedTable == null && availableTables.Any())
            {
                assignedTable = availableTables.OrderBy(t => t.Capacity).First();
            }
        }

        if (assignedTable == null)
        {
            throw new InvalidOperationException("No available tables for the requested date and party size.");
        }

        // Generate confirmation code
        var confirmationCode = GenerateConfirmationCode();

        // Create reservation
        var reservation = new Reservation
        {
            CustomerId = createDto.CustomerId,
            TableId = assignedTable.TableId,
            ReservationDate = createDto.ReservationDate,
            PartySize = createDto.PartySize,
            Duration = createDto.Duration,
            SpecialRequests = createDto.SpecialRequests,
            Status = "Confirmed",
            IsConfirmed = true,
            ConfirmationCode = confirmationCode,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        var createdReservation = await _reservationRepository.AddAsync(reservation);

        return new ReservationConfirmationDto
        {
            ReservationId = createdReservation.ReservationId,
            ConfirmationCode = confirmationCode,
            TableNumber = assignedTable.TableNumber,
            ReservationDate = createDto.ReservationDate,
            PartySize = createDto.PartySize,
            CustomerName = $"{customer.FirstName} {customer.LastName}",
            Message = "Reservation confirmed successfully!"
        };
    }

    public async Task<ReservationDto> UpdateReservationAsync(int id, UpdateReservationDto updateDto)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation with ID {id} not found.");
        }

        // Cannot update cancelled or completed reservations
        if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
        {
            throw new InvalidOperationException($"Cannot update a {reservation.Status.ToLower()} reservation.");
        }

        // Validate new date is in the future
        if (updateDto.ReservationDate <= DateTime.Now)
        {
            throw new InvalidOperationException("Reservation date must be in the future.");
        }

        // Check if table change is needed or requested
        var needsNewTable = false;
        if (updateDto.TableId.HasValue && updateDto.TableId.Value != reservation.TableId)
        {
            needsNewTable = true;
        }
        else if (updateDto.ReservationDate != reservation.ReservationDate ||
                 updateDto.Duration != reservation.Duration)
        {
            // Check if current table is still available for new time
            var isCurrentTableAvailable = await _reservationRepository.HasConflictingReservationAsync(
                reservation.TableId,
                updateDto.ReservationDate,
                updateDto.Duration,
                id);

            if (isCurrentTableAvailable)
            {
                needsNewTable = true;
            }
        }

        if (needsNewTable)
        {
            Table? newTable = null;

            if (updateDto.TableId.HasValue)
            {
                newTable = await _tableRepository.GetByIdAsync(updateDto.TableId.Value);
                if (newTable == null || !newTable.IsActive)
                {
                    throw new InvalidOperationException("Requested table is not available.");
                }

                var isAvailable = await _tableRepository.IsTableAvailableAsync(
                    updateDto.TableId.Value,
                    updateDto.ReservationDate,
                    updateDto.Duration);

                if (!isAvailable)
                {
                    throw new InvalidOperationException("Requested table is not available for the specified time.");
                }
            }
            else
            {
                var availableTables = await _tableRepository.GetAvailableTablesAsync(
                    updateDto.ReservationDate,
                    updateDto.PartySize);

                newTable = availableTables.OrderBy(t => t.Capacity).FirstOrDefault();

                if (newTable == null)
                {
                    throw new InvalidOperationException("No available tables for the requested date and party size.");
                }
            }

            reservation.TableId = newTable.TableId;
        }

        reservation.ReservationDate = updateDto.ReservationDate;
        reservation.PartySize = updateDto.PartySize;
        reservation.Duration = updateDto.Duration;
        reservation.SpecialRequests = updateDto.SpecialRequests;
        reservation.ModifiedDate = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation);

        // Reload to get navigation properties
        reservation = await _reservationRepository.GetByIdAsync(id);
        return MapToDto(reservation!);
    }

    public async Task<bool> CancelReservationAsync(int id, string? reason = null)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return false;
        }

        if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
        {
            throw new InvalidOperationException($"Cannot cancel a {reservation.Status.ToLower()} reservation.");
        }

        // Check if cancellation is too close to reservation time (less than 2 hours)
        var hoursUntilReservation = (reservation.ReservationDate - DateTime.Now).TotalHours;
        if (hoursUntilReservation < 2)
        {
            throw new InvalidOperationException("Reservations cannot be cancelled less than 2 hours before the scheduled time.");
        }

        reservation.Status = "Cancelled";
        reservation.CancelledDate = DateTime.UtcNow;
        reservation.CancellationReason = reason;
        reservation.ModifiedDate = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation);
        return true;
    }

    public async Task<bool> ConfirmReservationAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return false;
        }

        if (reservation.Status != "Pending")
        {
            throw new InvalidOperationException("Only pending reservations can be confirmed.");
        }

        reservation.Status = "Confirmed";
        reservation.IsConfirmed = true;
        reservation.ModifiedDate = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation);
        return true;
    }

    public async Task<bool> SeatReservationAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return false;
        }

        if (reservation.Status != "Confirmed")
        {
            throw new InvalidOperationException("Only confirmed reservations can be seated.");
        }

        reservation.Status = "Seated";
        reservation.ModifiedDate = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation);
        return true;
    }

    public async Task<bool> CompleteReservationAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return false;
        }

        if (reservation.Status != "Seated")
        {
            throw new InvalidOperationException("Only seated reservations can be completed.");
        }

        reservation.Status = "Completed";
        reservation.ModifiedDate = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation);
        return true;
    }

    public async Task<bool> MarkAsNoShowAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return false;
        }

        if (reservation.Status != "Confirmed")
        {
            throw new InvalidOperationException("Only confirmed reservations can be marked as no-show.");
        }

        // Check if reservation time has passed
        if (reservation.ReservationDate > DateTime.Now)
        {
            throw new InvalidOperationException("Cannot mark future reservations as no-show.");
        }

        reservation.Status = "NoShow";
        reservation.ModifiedDate = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation);
        return true;
    }

    public async Task<AvailabilityCheckDto> CheckAvailabilityAsync(DateTime date, int partySize)
    {
        var result = new AvailabilityCheckDto { Date = date.Date };

        // Generate time slots (e.g., every 30 minutes from 11 AM to 10 PM)
        var startHour = 11;
        var endHour = 22;
        var slotInterval = 30;

        for (var hour = startHour; hour < endHour; hour++)
        {
            for (var minute = 0; minute < 60; minute += slotInterval)
            {
                var slotTime = date.Date.AddHours(hour).AddMinutes(minute);

                var availableTables = await _tableRepository.GetAvailableTablesAsync(slotTime, partySize);

                result.AvailableSlots.Add(new TimeSlotAvailabilityDto
                {
                    Time = slotTime,
                    AvailableTables = availableTables.Count(),
                    AvailableTableIds = availableTables.Select(t => t.TableId).ToList()
                });
            }
        }

        return result;
    }

    private ReservationDto MapToDto(Reservation reservation)
    {
        return new ReservationDto
        {
            ReservationId = reservation.ReservationId,
            Customer = new CustomerInfoDto
            {
                CustomerId = reservation.Customer.CustomerId,
                FullName = $"{reservation.Customer.FirstName} {reservation.Customer.LastName}",
                Phone = reservation.Customer.Phone,
                Email = reservation.Customer.Email
            },
            Table = new TableInfoDto
            {
                TableId = reservation.Table.TableId,
                TableNumber = reservation.Table.TableNumber,
                Location = reservation.Table.Location
            },
            ReservationDate = reservation.ReservationDate,
            PartySize = reservation.PartySize,
            Status = reservation.Status,
            SpecialRequests = reservation.SpecialRequests,
            Duration = reservation.Duration,
            IsConfirmed = reservation.IsConfirmed,
            ConfirmationCode = reservation.ConfirmationCode,
            CreatedDate = reservation.CreatedDate
        };
    }

    private string GenerateConfirmationCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}