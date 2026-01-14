using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Application.DTOs.Reservation;

public class UpdateReservationDto
{
    public DateTime ReservationDate { get; set; }
    public int PartySize { get; set; }
    public int Duration { get; set; }
    public string? SpecialRequests { get; set; }
    public int? TableId { get; set; }
}
