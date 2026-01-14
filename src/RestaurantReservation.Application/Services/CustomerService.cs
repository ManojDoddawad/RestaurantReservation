using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantReservation.Application.DTOs.Customer;
using RestaurantReservation.Application.DTOs.Common;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Interfaces;

namespace RestaurantReservation.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedResult<CustomerDto>> GetAllCustomersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null)
    {
        var (items, totalCount) = await _customerRepository.GetPagedAsync(pageNumber, pageSize, searchTerm);

        var customerDtos = items.Select(c => MapToDto(c)).ToList();

        return new PagedResult<CustomerDto>
        {
            Data = customerDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount
        };
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer != null ? MapToDto(customer) : null;
    }

    public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
    {
        var customer = await _customerRepository.GetByEmailAsync(email);
        return customer != null ? MapToDto(customer) : null;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createDto)
    {
        // Check if email already exists
        var existingCustomer = await _customerRepository.GetByEmailAsync(createDto.Email);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException("A customer with this email already exists.");
        }

        var customer = new Customer
        {
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Email = createDto.Email,
            Phone = createDto.Phone,
            DateOfBirth = createDto.DateOfBirth,
            DietaryRestrictions = createDto.DietaryRestrictions,
            IsVIP = false,
            IsBlacklisted = false,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        var createdCustomer = await _customerRepository.AddAsync(customer);
        return MapToDto(createdCustomer);
    }

    public async Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto updateDto)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {id} not found.");
        }

        // Check if email is being changed and if new email already exists
        if (customer.Email != updateDto.Email)
        {
            var existingCustomer = await _customerRepository.GetByEmailAsync(updateDto.Email);
            if (existingCustomer != null)
            {
                throw new InvalidOperationException("A customer with this email already exists.");
            }
        }

        customer.FirstName = updateDto.FirstName;
        customer.LastName = updateDto.LastName;
        customer.Email = updateDto.Email;
        customer.Phone = updateDto.Phone;
        customer.DateOfBirth = updateDto.DateOfBirth;
        customer.DietaryRestrictions = updateDto.DietaryRestrictions;
        customer.IsVIP = updateDto.IsVIP;
        customer.ModifiedDate = DateTime.UtcNow;

        await _customerRepository.UpdateAsync(customer);
        return MapToDto(customer);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return false;
        }

        await _customerRepository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> CustomerExistsAsync(int id)
    {
        return await _customerRepository.ExistsAsync(id);
    }

    private CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            DateOfBirth = customer.DateOfBirth,
            DietaryRestrictions = customer.DietaryRestrictions,
            IsVIP = customer.IsVIP,
            TotalReservations = customer.Reservations?.Count ?? 0,
            LastReservationDate = customer.Reservations?
                .OrderByDescending(r => r.ReservationDate)
                .FirstOrDefault()?.ReservationDate
        };
    }
}