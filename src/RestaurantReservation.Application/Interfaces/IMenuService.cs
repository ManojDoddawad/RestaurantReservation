using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantReservation.Application.DTOs.Menu;
using RestaurantReservation.Application.DTOs.Common;

namespace RestaurantReservation.Application.Interfaces;

public interface IMenuService
{
    // Categories
    Task<IEnumerable<MenuCategoryDto>> GetAllCategoriesAsync();
    Task<IEnumerable<MenuCategoryDto>> GetActiveCategoriesAsync();
    Task<MenuCategoryDto?> GetCategoryByIdAsync(int id);
    Task<MenuCategoryDto> CreateCategoryAsync(CreateMenuCategoryDto createDto);
    Task<MenuCategoryDto> UpdateCategoryAsync(int id, UpdateMenuCategoryDto updateDto);
    Task<bool> DeleteCategoryAsync(int id);

    // Menu Items
    Task<PagedResult<MenuItemDto>> GetAllMenuItemsAsync(
        int pageNumber,
        int pageSize,
        int? categoryId = null,
        bool? isAvailable = null,
        string? dietaryTag = null);
    Task<MenuItemDto?> GetMenuItemByIdAsync(int id);
    Task<IEnumerable<MenuItemDto>> GetItemsByCategoryAsync(int categoryId);
    Task<IEnumerable<MenuItemDto>> GetAvailableItemsAsync();
    Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto createDto);
    Task<MenuItemDto> UpdateMenuItemAsync(int id, UpdateMenuItemDto updateDto);
    Task<bool> DeleteMenuItemAsync(int id);
}
