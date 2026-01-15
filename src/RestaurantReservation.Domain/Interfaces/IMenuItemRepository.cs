using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Domain.Interfaces;

public interface IMenuItemRepository : IGenericRepository<MenuItem>
{
    Task<IEnumerable<MenuItem>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<MenuItem>> GetAvailableItemsAsync();
    Task<IEnumerable<MenuItem>> GetByDietaryTagAsync(string tag);
    Task<(IEnumerable<MenuItem> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? categoryId = null,
        bool? isAvailable = null,
        string? dietaryTag = null);
}