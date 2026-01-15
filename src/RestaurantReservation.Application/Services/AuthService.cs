using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestaurantReservation.Application.DTOs.Auth;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Interfaces;
using BCrypt.Net;

namespace RestaurantReservation.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Validate username
        if (await _userRepository.UsernameExistsAsync(registerDto.Username))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        // Validate email
        if (await _userRepository.EmailExistsAsync(registerDto.Email))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        // Validate password strength
        if (registerDto.Password.Length < 6)
        {
            throw new InvalidOperationException("Password must be at least 6 characters long.");
        }

        // Validate role
        var validRoles = new[] { "Customer", "Staff", "Admin" };
        if (!validRoles.Contains(registerDto.Role))
        {
            throw new InvalidOperationException("Invalid role. Must be Customer, Staff, or Admin.");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // Create user
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            Role = registerDto.Role,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);

        // Generate JWT token
        var token = _jwtService.GenerateToken(createdUser);
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "1440");

        return new AuthResponseDto
        {
            UserId = createdUser.UserId,
            Username = createdUser.Username,
            Email = createdUser.Email,
            Role = createdUser.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Find user by username
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive.");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        // Update last login date
        user.LastLoginDate = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "1440");

        return new AuthResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        // Validate new password
        if (changePasswordDto.NewPassword.Length < 6)
        {
            throw new InvalidOperationException("New password must be at least 6 characters long.");
        }

        // Hash new password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await _userRepository.UsernameExistsAsync(username);
    }
}
