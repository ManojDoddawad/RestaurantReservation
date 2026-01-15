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

public class MenuCategoryRepository : GenericRepository<MenuCategory>, IMenuCategoryRepository
{
    public MenuCategoryRepository(RestaurantDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MenuCategory>> GetActiveCategoriesAsync()
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Include(c => c.MenuItems)
            .ToListAsync();
    }

    public async Task<MenuCategory?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Include(c => c.MenuItems)
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
    }

    public override async Task<MenuCategory?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(c => c.MenuItems)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }
}