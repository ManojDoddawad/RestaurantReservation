using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.DTOs.Menu;
using RestaurantReservation.Application.DTOs.Common;
using RestaurantReservation.Application.Interfaces;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly ILogger<MenuController> _logger;

    public MenuController(
        IMenuService menuService,
        ILogger<MenuController> logger)
    {
        _menuService = menuService;
        _logger = logger;
    }

    // ========== CATEGORY ENDPOINTS ==========

    /// <summary>
    /// Get all menu categories
    /// </summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MenuCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MenuCategoryDto>>>> GetAllCategories()
    {
        try
        {
            var categories = await _menuService.GetAllCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<MenuCategoryDto>>.SuccessResponse(categories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, ApiResponse<IEnumerable<MenuCategoryDto>>.ErrorResponse(
                "An error occurred while retrieving categories"));
        }
    }

    /// <summary>
    /// Get active categories only
    /// </summary>
    [HttpGet("categories/active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MenuCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MenuCategoryDto>>>> GetActiveCategories()
    {
        try
        {
            var categories = await _menuService.GetActiveCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<MenuCategoryDto>>.SuccessResponse(categories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active categories");
            return StatusCode(500, ApiResponse<IEnumerable<MenuCategoryDto>>.ErrorResponse(
                "An error occurred while retrieving categories"));
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("categories/{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<MenuCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MenuCategoryDto>>> GetCategoryById(int id)
    {
        try
        {
            var category = await _menuService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(ApiResponse<MenuCategoryDto>.ErrorResponse(
                    $"Category with ID {id} not found"));
            }
            return Ok(ApiResponse<MenuCategoryDto>.SuccessResponse(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category");
            return StatusCode(500, ApiResponse<MenuCategoryDto>.ErrorResponse(
                "An error occurred while retrieving the category"));
        }
    }

    /// <summary>
    /// Create a new category (Admin/Staff only)
    /// </summary>
    [HttpPost("categories")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<MenuCategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<MenuCategoryDto>>> CreateCategory(
        [FromBody] CreateMenuCategoryDto createDto)
    {
        try
        {
            var category = await _menuService.CreateCategoryAsync(createDto);
            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = category.CategoryId },
                ApiResponse<MenuCategoryDto>.SuccessResponse(category, "Category created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuCategoryDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, ApiResponse<MenuCategoryDto>.ErrorResponse(
                "An error occurred while creating the category"));
        }
    }

    /// <summary>
    /// Update a category (Admin/Staff only)
    /// </summary>
    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<MenuCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MenuCategoryDto>>> UpdateCategory(
        int id,
        [FromBody] UpdateMenuCategoryDto updateDto)
    {
        try
        {
            var category = await _menuService.UpdateCategoryAsync(id, updateDto);
            return Ok(ApiResponse<MenuCategoryDto>.SuccessResponse(category, "Category updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuCategoryDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MenuCategoryDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return StatusCode(500, ApiResponse<MenuCategoryDto>.ErrorResponse(
                "An error occurred while updating the category"));
        }
    }

    /// <summary>
    /// Delete a category (Admin only)
    /// </summary>
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var result = await _menuService.DeleteCategoryAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Category with ID {id} not found"));
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while deleting the category"));
        }
    }

    // ========== MENU ITEM ENDPOINTS ==========

    /// <summary>
    /// Get all menu items with filters
    /// </summary>
    [HttpGet("items")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MenuItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<MenuItemDto>>>> GetAllMenuItems(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? categoryId = null,
        [FromQuery] bool? isAvailable = null,
        [FromQuery] string? dietaryTag = null)
    {
        try
        {
            var result = await _menuService.GetAllMenuItemsAsync(
                pageNumber, pageSize, categoryId, isAvailable, dietaryTag);
            return Ok(ApiResponse<PagedResult<MenuItemDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting menu items");
            return StatusCode(500, ApiResponse<PagedResult<MenuItemDto>>.ErrorResponse(
                "An error occurred while retrieving menu items"));
        }
    }

    /// <summary>
    /// Get available menu items only
    /// </summary>
    [HttpGet("items/available")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MenuItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MenuItemDto>>>> GetAvailableItems()
    {
        try
        {
            var items = await _menuService.GetAvailableItemsAsync();
            return Ok(ApiResponse<IEnumerable<MenuItemDto>>.SuccessResponse(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available items");
            return StatusCode(500, ApiResponse<IEnumerable<MenuItemDto>>.ErrorResponse(
                "An error occurred while retrieving menu items"));
        }
    }

    /// <summary>
    /// Get menu item by ID
    /// </summary>
    [HttpGet("items/{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<MenuItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> GetMenuItemById(int id)
    {
        try
        {
            var item = await _menuService.GetMenuItemByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<MenuItemDto>.ErrorResponse(
                    $"Menu item with ID {id} not found"));
            }
            return Ok(ApiResponse<MenuItemDto>.SuccessResponse(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting menu item");
            return StatusCode(500, ApiResponse<MenuItemDto>.ErrorResponse(
                "An error occurred while retrieving the menu item"));
        }
    }

    /// <summary>
    /// Get items by category
    /// </summary>
    [HttpGet("categories/{categoryId}/items")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MenuItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MenuItemDto>>>> GetItemsByCategory(int categoryId)
    {
        try
        {
            var items = await _menuService.GetItemsByCategoryAsync(categoryId);
            return Ok(ApiResponse<IEnumerable<MenuItemDto>>.SuccessResponse(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting items by category");
            return StatusCode(500, ApiResponse<IEnumerable<MenuItemDto>>.ErrorResponse(
                "An error occurred while retrieving menu items"));
        }
    }

    /// <summary>
    /// Create a new menu item (Admin/Staff only)
    /// </summary>
    [HttpPost("items")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<MenuItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> CreateMenuItem(
        [FromBody] CreateMenuItemDto createDto)
    {
        try
        {
            var item = await _menuService.CreateMenuItemAsync(createDto);
            return CreatedAtAction(
                nameof(GetMenuItemById),
                new { id = item.MenuItemId },
                ApiResponse<MenuItemDto>.SuccessResponse(item, "Menu item created successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuItemDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu item");
            return StatusCode(500, ApiResponse<MenuItemDto>.ErrorResponse(
                "An error occurred while creating the menu item"));
        }
    }

    /// <summary>
    /// Update a menu item (Admin/Staff only)
    /// </summary>
    [HttpPut("items/{id}")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<MenuItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> UpdateMenuItem(
        int id,
        [FromBody] UpdateMenuItemDto updateDto)
    {
        try
        {
            var item = await _menuService.UpdateMenuItemAsync(id, updateDto);
            return Ok(ApiResponse<MenuItemDto>.SuccessResponse(item, "Menu item updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MenuItemDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu item");
            return StatusCode(500, ApiResponse<MenuItemDto>.ErrorResponse(
                "An error occurred while updating the menu item"));
        }
    }

    /// <summary>
    /// Delete a menu item (Admin only)
    /// </summary>
    [HttpDelete("items/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMenuItem(int id)
    {
        try
        {
            var result = await _menuService.DeleteMenuItemAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Menu item with ID {id} not found"));
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu item");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while deleting the menu item"));
        }
    }
}