using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using AuthService.Domain.ValueObjects;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using AuthService.Application.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace AuthService.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

        if (request.Password != request.ConfirmPassword)
        {
            _logger.LogWarning("Registration failed: Passwords do not match");
            throw new ArgumentException("The passwords don't match");
        }

        var trimmedEmail = request.Email.Trim();

        bool exists = await _userRepository.ExistsByEmailAsync(trimmedEmail);

        if (exists)
        {
            _logger.LogWarning("Registration failed: User with email {Email} already exists", request.Email);
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var emailVo = new EmailVO(trimmedEmail);
        var passwordVo = new PasswordVO(request.Password);

        var user = new User(
            email: emailVo,
            password: passwordVo,
            role: Roles.Customer
        );

        await _userRepository.CreateAsync(user);

        _logger.LogInformation("User successfully registered with ID: {UserId}", user.Id);

        return new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email.Value, 
            Message = "Registration was successful"
        };
    }


    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Attempting to login user with email: {Email}", request.Email);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login failed: Email or password is empty");
            throw new ArgumentException("Email and password are required.");
        }

        var lowerEmail = request.Email.ToLower().Trim();

        var user = await _userRepository.GetByEmailAsync(lowerEmail);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User with email {Email} not found", request.Email);
            throw new UnauthorizedAccessException("Incorrect email or password");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: User account {UserId} is inactive", user.Id);
            throw new UnauthorizedAccessException("The account is inactive");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash!.Hash))
        {
            _logger.LogWarning("Login failed: Invalid password for user {Email}", request.Email);
            throw new UnauthorizedAccessException("Incorrect email or password");
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);

        _logger.LogInformation("User {UserId} successfully logged in", user.Id);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UserId = user.Id,
            Email = user.Email.Value,
            Role = user.Role.ToString(),
        };
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
    {
        _logger.LogInformation("Attempting to get list of all users");
        var users = await _userRepository.GetAllAsync();

        _logger.LogInformation("List of all users successfully loaded");

        return users.Select(user => new UserDTO
        {
            UserId = user.Id,
            Email = user.Email.Value,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        });
        
    }





    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        _logger.LogInformation("Attempting to refresh token");

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _logger.LogWarning("Refresh failed: Token is empty");
            throw new ArgumentException("Refresh token is required");
        }

        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh failed: Token not found");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if (!refreshToken.IsActive())
        {
            _logger.LogWarning("Refresh failed: Token is not active");
            throw new UnauthorizedAccessException("Refresh token is invalid or revoked");
        }

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Refresh failed: User not found or inactive");
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(refreshToken);

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(newRefreshToken);

        _logger.LogInformation("Token successfully refreshed for user {UserId}", user.Id);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task LogoutAsync(Guid userId)
    {
        _logger.LogInformation("Attempting to logout user {UserId}", userId);

        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);

        _logger.LogInformation("User {UserId} successfully logged out", userId);
    }
    public async Task DeleteAsync(Guid userId)
    {
        _logger.LogInformation("Attempting to delete user {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        await _userRepository.DeleteAsync(userId);

        _logger.LogInformation("User {UserId} successfully deleted", userId);
    }
}
