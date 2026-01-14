using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Application.DTOs.Reservation;

public class AvailabilityCheckDto
{
    public DateTime Date { get; set; }
    public List<TimeSlotAvailabilityDto> AvailableSlots { get; set; } = new();
}
public class TimeSlotAvailabilityDto
{
    public DateTime Time { get; set; }
    public int AvailableTables { get; set; }
    public List<int> AvailableTableIds { get; set; } = new();
}