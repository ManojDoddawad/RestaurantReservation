using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestaurantReservation.Application.DTOs.Customer;
using RestaurantReservation.Application.DTOs.Common;

namespace RestaurantReservation.Application.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetAllCustomersAsync(int pageNumber, int pageSize, string? searchTerm = null);
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto?> GetCustomerByEmailAsync(string email);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createDto);
    Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto updateDto);
    Task<bool> DeleteCustomerAsync(int id);
    Task<bool> CustomerExistsAsync(int id);
}
