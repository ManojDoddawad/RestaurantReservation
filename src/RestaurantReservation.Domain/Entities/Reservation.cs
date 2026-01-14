using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Domain.Entities;

public class Reservation
{
    public int ReservationId { get; set; }
    public int CustomerId { get; set; }
    public int TableId { get; set; }
    public DateTime ReservationDate { get; set; }
    public int PartySize { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SpecialRequests { get; set; }
    public int Duration { get; set; }
    public bool IsConfirmed { get; set; }
    public string? ConfirmationCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public Table Table { get; set; } = null!;
    public ICollection<ReservationLog> ReservationLogs { get; set; } = new List<ReservationLog>();
}
