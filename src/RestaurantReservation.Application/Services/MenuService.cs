using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantReservation.Application.DTOs.Menu;
using RestaurantReservation.Application.DTOs.Common;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Interfaces;

namespace RestaurantReservation.Application.Services;

public class MenuService : IMenuService
{
    private readonly IMenuCategoryRepository _categoryRepository;
    private readonly IMenuItemRepository _menuItemRepository;

    public MenuService(
        IMenuCategoryRepository categoryRepository,
        IMenuItemRepository menuItemRepository)
    {
        _categoryRepository = categoryRepository;
        _menuItemRepository = menuItemRepository;
    }

    // Category Methods
    public async Task<IEnumerable<MenuCategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => MapCategoryToDto(c));
    }

    public async Task<IEnumerable<MenuCategoryDto>> GetActiveCategoriesAsync()
    {
        var categories = await _categoryRepository.GetActiveCategoriesAsync();
        return categories.Select(c => MapCategoryToDto(c));
    }

    public async Task<MenuCategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null ? MapCategoryToDto(category) : null;
    }

    public async Task<MenuCategoryDto> CreateCategoryAsync(CreateMenuCategoryDto createDto)
    {
        var existingCategory = await _categoryRepository.GetByNameAsync(createDto.Name);
        if (existingCategory != null)
        {
            throw new InvalidOperationException($"Category '{createDto.Name}' already exists.");
        }

        var category = new MenuCategory
        {
            Name = createDto.Name,
            Description = createDto.Description,
            DisplayOrder = createDto.DisplayOrder,
            IsActive = true
        };

        var createdCategory = await _categoryRepository.AddAsync(category);
        return MapCategoryToDto(createdCategory);
    }

    public async Task<MenuCategoryDto> UpdateCategoryAsync(int id, UpdateMenuCategoryDto updateDto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {id} not found.");
        }

        if (category.Name != updateDto.Name)
        {
            var existingCategory = await _categoryRepository.GetByNameAsync(updateDto.Name);
            if (existingCategory != null)
            {
                throw new InvalidOperationException($"Category '{updateDto.Name}' already exists.");
            }
        }

        category.Name = updateDto.Name;
        category.Description = updateDto.Description;
        category.DisplayOrder = updateDto.DisplayOrder;
        category.IsActive = updateDto.IsActive;

        await _categoryRepository.UpdateAsync(category);
        return MapCategoryToDto(category);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return false;
        }

        if (category.MenuItems?.Any() == true)
        {
            throw new InvalidOperationException("Cannot delete category with existing menu items.");
        }

        await _categoryRepository.DeleteAsync(id);
        return true;
    }

    // Menu Item Methods
    public async Task<PagedResult<MenuItemDto>> GetAllMenuItemsAsync(
        int pageNumber,
        int pageSize,
        int? categoryId = null,
        bool? isAvailable = null,
        string? dietaryTag = null)
    {
        var (items, totalCount) = await _menuItemRepository.GetPagedAsync(
            pageNumber, pageSize, categoryId, isAvailable, dietaryTag);

        var menuItemDtos = items.Select(m => MapMenuItemToDto(m)).ToList();

        return new PagedResult<MenuItemDto>
        {
            Data = menuItemDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount
        };
    }

    public async Task<MenuItemDto?> GetMenuItemByIdAsync(int id)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id);
        return menuItem != null ? MapMenuItemToDto(menuItem) : null;
    }

    public async Task<IEnumerable<MenuItemDto>> GetItemsByCategoryAsync(int categoryId)
    {
        var items = await _menuItemRepository.GetByCategoryIdAsync(categoryId);
        return items.Select(m => MapMenuItemToDto(m));
    }

    public async Task<IEnumerable<MenuItemDto>> GetAvailableItemsAsync()
    {
        var items = await _menuItemRepository.GetAvailableItemsAsync();
        return items.Select(m => MapMenuItemToDto(m));
    }

    public async Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto createDto)
    {
        var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {createDto.CategoryId} not found.");
        }

        var menuItem = new MenuItem
        {
            CategoryId = createDto.CategoryId,
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            IsAvailable = true,
            DietaryTags = createDto.DietaryTags,
            ImageUrl = createDto.ImageUrl,
            PreparationTime = createDto.PreparationTime,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        var createdItem = await _menuItemRepository.AddAsync(menuItem);

        // Reload to get navigation properties
        createdItem = await _menuItemRepository.GetByIdAsync(createdItem.MenuItemId);
        return MapMenuItemToDto(createdItem!);
    }

    public async Task<MenuItemDto> UpdateMenuItemAsync(int id, UpdateMenuItemDto updateDto)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id);
        if (menuItem == null)
        {
            throw new KeyNotFoundException($"Menu item with ID {id} not found.");
        }

        var category = await _categoryRepository.GetByIdAsync(updateDto.CategoryId);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {updateDto.CategoryId} not found.");
        }

        menuItem.CategoryId = updateDto.CategoryId;
        menuItem.Name = updateDto.Name;
        menuItem.Description = updateDto.Description;
        menuItem.Price = updateDto.Price;
        menuItem.IsAvailable = updateDto.IsAvailable;
        menuItem.DietaryTags = updateDto.DietaryTags;
        menuItem.ImageUrl = updateDto.ImageUrl;
        menuItem.PreparationTime = updateDto.PreparationTime;
        menuItem.ModifiedDate = DateTime.UtcNow;

        await _menuItemRepository.UpdateAsync(menuItem);

        // Reload to get navigation properties
        menuItem = await _menuItemRepository.GetByIdAsync(id);
        return MapMenuItemToDto(menuItem!);
    }

    public async Task<bool> DeleteMenuItemAsync(int id)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id);
        if (menuItem == null)
        {
            return false;
        }

        await _menuItemRepository.DeleteAsync(id);
        return true;
    }

    private MenuCategoryDto MapCategoryToDto(MenuCategory category)
    {
        return new MenuCategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ItemCount = category.MenuItems?.Count ?? 0
        };
    }

    private MenuItemDto MapMenuItemToDto(MenuItem menuItem)
    {
        return new MenuItemDto
        {
            MenuItemId = menuItem.MenuItemId,
            CategoryId = menuItem.CategoryId,
            CategoryName = menuItem.Category?.Name ?? string.Empty,
            Name = menuItem.Name,
            Description = menuItem.Description,
            Price = menuItem.Price,
            IsAvailable = menuItem.IsAvailable,
            DietaryTags = string.IsNullOrWhiteSpace(menuItem.DietaryTags)
                ? new List<string>()
                : menuItem.DietaryTags.Split(',').Select(t => t.Trim()).ToList(),
            ImageUrl = menuItem.ImageUrl,
            PreparationTime = menuItem.PreparationTime
        };
    }
}