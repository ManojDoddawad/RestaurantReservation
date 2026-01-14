using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Domain.Entities;

public class ReservationLog
{
    public int LogId { get; set; }
    public int ReservationId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldStatus { get; set; }
    public string? NewStatus { get; set; }
    public int? ChangedBy { get; set; }
    public DateTime ChangedDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Reservation Reservation { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}
