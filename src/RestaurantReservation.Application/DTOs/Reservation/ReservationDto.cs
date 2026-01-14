using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Application.DTOs.Reservation;

public class ReservationDto
{
    public int ReservationId { get; set; }
    public CustomerInfoDto Customer { get; set; } = new();
    public TableInfoDto Table { get; set; } = new();
    public DateTime ReservationDate { get; set; }
    public int PartySize { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SpecialRequests { get; set; }
    public int Duration { get; set; }
    public bool IsConfirmed { get; set; }
    public string? ConfirmationCode { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CustomerInfoDto
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class TableInfoDto
{
    public int TableId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public string? Location { get; set; }
}