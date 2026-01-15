using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Application.DTOs.Menu;

public class CreateMenuItemDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? DietaryTags { get; set; }
    public string? ImageUrl { get; set; }
    public int? PreparationTime { get; set; }
}