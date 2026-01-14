using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Application.DTOs.Reservation;

public class ReservationConfirmationDto
{
    public int ReservationId { get; set; }
    public string ConfirmationCode { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public int PartySize { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
