using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Application.DTOs.Table;

public class UpdateTableDto
{
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public int? MinimumCapacity { get; set; }
    public int? MaximumCapacity { get; set; }
}
