using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Domain.Interfaces;

public interface IMenuCategoryRepository : IGenericRepository<MenuCategory>
{
    Task<IEnumerable<MenuCategory>> GetActiveCategoriesAsync();
    Task<MenuCategory?> GetByNameAsync(string name);
}