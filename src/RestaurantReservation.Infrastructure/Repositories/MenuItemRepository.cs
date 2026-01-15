using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Interfaces;
using RestaurantReservation.Infrastructure.Data;

namespace RestaurantReservation.Infrastructure.Repositories;

public class MenuItemRepository : GenericRepository<MenuItem>, IMenuItemRepository
{
    public MenuItemRepository(RestaurantDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MenuItem>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Include(m => m.Category)
            .Where(m => m.CategoryId == categoryId)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetAvailableItemsAsync()
    {
        return await _dbSet
            .Include(m => m.Category)
            .Where(m => m.IsAvailable && m.Category.IsActive)
            .OrderBy(m => m.Category.DisplayOrder)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetByDietaryTagAsync(string tag)
    {
        return await _dbSet
            .Include(m => m.Category)
            .Where(m => m.DietaryTags != null && m.DietaryTags.Contains(tag) && m.IsAvailable)
            .ToListAsync();
    }

    public async Task<(IEnumerable<MenuItem> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? categoryId = null,
        bool? isAvailable = null,
        string? dietaryTag = null)
    {
        var query = _dbSet.Include(m => m.Category).AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(m => m.IsAvailable == isAvailable.Value);
        }

        if (!string.IsNullOrWhiteSpace(dietaryTag))
        {
            query = query.Where(m => m.DietaryTags != null && m.DietaryTags.Contains(dietaryTag));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(m => m.Category.DisplayOrder)
            .ThenBy(m => m.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public override async Task<MenuItem?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.MenuItemId == id);
    }
}