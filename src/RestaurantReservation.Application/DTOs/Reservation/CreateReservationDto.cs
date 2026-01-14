using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Application.DTOs.Reservation;

public class CreateReservationDto
{
    public int CustomerId { get; set; }
    public DateTime ReservationDate { get; set; }
    public int PartySize { get; set; }
    public int Duration { get; set; } = 120;
    public string? SpecialRequests { get; set; }
    public string? PreferredTableLocation { get; set; }
    public int? PreferredTableId { get; set; }
}

