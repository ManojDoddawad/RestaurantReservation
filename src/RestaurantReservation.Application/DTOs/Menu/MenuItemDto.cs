using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RestaurantReservation.Application.DTOs.Menu;

public class MenuItemDto
{
    public int MenuItemId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> DietaryTags { get; set; } = new();
    public string? ImageUrl { get; set; }
    public int? PreparationTime { get; set; }
}