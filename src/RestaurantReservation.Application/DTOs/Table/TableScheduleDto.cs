using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Application.DTOs.Table;

public class TableScheduleDto
{
    public string TableNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<ScheduleSlotDto> Schedule { get; set; } = new();
}

public class ScheduleSlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ReservationId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int PartySize { get; set; }
    public string Status { get; set; } = string.Empty;
}
