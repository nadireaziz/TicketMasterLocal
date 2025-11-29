using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Application.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User)> RegisterAsync(string email, string password, string fullName, string? phone);
    Task<(bool Success, string Message, string AccessToken, string RefreshToken, User? User)> LoginAsync(string email, string password, string? deviceName, string? ipAddress);
    Task<(bool Success, string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<int> GetActiveSessionCountAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private const int MAX_DEVICES = 5;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(
        string email, string password, string fullName, string? phone)
    {
        // Check if user exists
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return (false, "Email already registered", null);
        }

        // Validate password strength
        if (password.Length < 8)
        {
            return (false, "Password must be at least 8 characters", null);
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Create user
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            PhoneNumber = phone,
            Role = "Customer",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (true, "Registration successful", user);
    }

    public async Task<(bool Success, string Message, string AccessToken, string RefreshToken, User? User)> LoginAsync(
        string email, string password, string? deviceName, string? ipAddress)
    {
        // Find user
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !user.IsActive)
        {
            return (false, "Invalid credentials", string.Empty, string.Empty, null);
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return (false, "Invalid credentials", string.Empty, string.Empty, null);
        }

        // Check active sessions (limit to 5 devices)
        var activeSessions = user.RefreshTokens.Count(t => t.IsActive);
        if (activeSessions >= MAX_DEVICES)
        {
            // Revoke oldest session
            var oldestToken = user.RefreshTokens
                .Where(t => t.IsActive)
                .OrderBy(t => t.CreatedAt)
                .First();
            oldestToken.RevokedAt = DateTime.UtcNow;
        }

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.UserId,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"]!)),
            DeviceName = deviceName,
            IpAddress = ipAddress
        };

        _context.RefreshTokens.Add(refreshTokenEntity);

        // Update last login
        user.LastLoginDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Login successful", accessToken, refreshToken, user);
    }

    public async Task<(bool Success, string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null || !token.IsActive)
        {
            return (false, string.Empty, string.Empty);
        }

        // Generate new tokens
        var newAccessToken = GenerateAccessToken(token.User);
        var newRefreshToken = GenerateRefreshToken();

        // Revoke old token and create new one
        token.RevokedAt = DateTime.UtcNow;

        var newTokenEntity = new RefreshToken
        {
            UserId = token.UserId,
            Token = newRefreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"]!)),
            DeviceName = token.DeviceName,
            IpAddress = token.IpAddress
        };

        _context.RefreshTokens.Add(newTokenEntity);
        await _context.SaveChangesAsync();

        return (true, newAccessToken, newRefreshToken);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null)
            return false;

        token.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetActiveSessionCountAsync(int userId)
    {
        return await _context.RefreshTokens
            .CountAsync(t => t.UserId == userId && t.IsActive);
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
