# 🔐 JWT Authentication Implementation Guide

## ✅ Already Completed
- ✅ User entity updated with auth fields
- ✅ RefreshToken entity created

## 📋 Remaining Implementation Steps

### 1. Update DbContext

Add to `AppDbContext.cs`:

```csharp
public DbSet<RefreshToken> RefreshTokens { get; set; }

// In OnModelCreating, add:
modelBuilder.Entity<User>(entity =>
{
    entity.Property(e => e.PasswordHash).IsRequired();
    entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
});

modelBuilder.Entity<RefreshToken>(entity =>
{
    entity.HasKey(e => e.TokenId);
    entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
    entity.HasIndex(e => e.Token).IsUnique();

    entity.HasOne(e => e.User)
          .WithMany(u => u.RefreshTokens)
          .HasForeignKey(e => e.UserId)
          .OnDelete(DeleteBehavior.Cascade);
});
```

### 2. Install NuGet Packages

```bash
cd TicketMasterLocal
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next
dotnet add package System.IdentityModel.Tokens.Jwt
```

### 3. Create DTOs

Create folder: `TicketMasterLocal/DTOs/Auth/`

**RegisterRequest.cs:**
```csharp
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
```

**LoginRequest.cs:**
```csharp
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
}
```

**TokenResponse.cs:**
```csharp
public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
```

### 4. Add JWT Configuration

Add to `appsettings.json`:
```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-must-be-at-least-32-characters-long-for-security",
    "Issuer": "TicketMaster",
    "Audience": "TicketMasterApp",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### 5. Create AuthService

Create: `TicketMaster.Application/Services/AuthService.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

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
            ExpiresAt = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenExpirationDays")),
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
            ExpiresAt = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenExpirationDays")),
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
            expires: DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:AccessTokenExpirationMinutes")),
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
```

### 6. Create AuthController

Create: `TicketMasterLocal/Controllers/AuthController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, message, user) = await _authService.RegisterAsync(
            request.Email, request.Password, request.FullName, request.PhoneNumber);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message, user = new { user.UserId, user.Email, user.FullName } });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, message, accessToken, refreshToken, user) = await _authService.LoginAsync(
            request.Email, request.Password, request.DeviceName, ipAddress);

        if (!success)
            return Unauthorized(new { message });

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            }
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var (success, accessToken, refreshToken) = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!success)
            return Unauthorized(new { message = "Invalid refresh token" });

        return Ok(new { accessToken, refreshToken, expiresAt = DateTime.UtcNow.AddMinutes(15) });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.RevokeTokenAsync(request.RefreshToken);
        return Ok(new { message = "Logged out successfully" });
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetActiveSessions()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var count = await _authService.GetActiveSessionCountAsync(userId);
        return Ok(new { activeSessions = count, maxAllowed = 5 });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            email = User.FindFirst(ClaimTypes.Email)?.Value,
            fullName = User.FindFirst(ClaimTypes.Name)?.Value,
            role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
```

### 7. Configure JWT in Program.cs

Add before `var app = builder.Build();`:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// ... existing code ...

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
    };
});

builder.Services.AddAuthorization();

// Register AuthService
builder.Services.AddScoped<IAuthService, AuthService>();
```

Add after `app.UseStaticFiles();`:

```csharp
app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();
```

### 8. Update DataSeeder

Update user seeding to include passwords:

```csharp
var users = new List<User>
{
    new User
    {
        Email = "john.doe@email.com",
        FullName = "John Doe",
        PhoneNumber = "+1-555-0101",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
        Role = "Customer"
    },
    new User
    {
        Email = "admin@ticketmaster.com",
        FullName = "Admin User",
        PhoneNumber = "+1-555-9999",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
        Role = "Admin"
    }
};
```

### 9. Protect Existing Endpoints

Add `[Authorize]` to controllers that need protection:

```csharp
// BookingsController.cs
[Authorize]
[HttpPost]
public async Task<IActionResult> CreateBooking(...) { }

// Only allow users to see their own bookings
[Authorize]
[HttpGet("my-bookings")]
public async Task<IActionResult> GetMyBookings()
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var bookings = await _context.Bookings
        .Where(b => b.UserId == userId)
        .Include(b => b.Event)
        .ToListAsync();
    return Ok(bookings);
}

// Admin only endpoints
[Authorize(Roles = "Admin")]
[HttpPost]
public async Task<IActionResult> CreateEvent(...) { }
```

##Testing the Authentication System

```bash
# 1. Register a new user
curl -X POST http://localhost:5277/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!","fullName":"Test User"}'

# 2. Login
curl -X POST http://localhost:5277/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!","deviceName":"Chrome on Mac"}'

# Save the access token from response

# 3. Access protected endpoint
curl http://localhost:5277/api/auth/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"

# 4. Refresh token
curl -X POST http://localhost:5277/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"YOUR_REFRESH_TOKEN"}'

# 5. Logout
curl -X POST http://localhost:5277/api/auth/logout \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"YOUR_REFRESH_TOKEN"}'
```

## 📝 Implementation Checklist

- [ ] Update DbContext with RefreshToken configuration
- [ ] Install NuGet packages
- [ ] Create DTOs folder and files
- [ ] Add JWT configuration to appsettings.json
- [ ] Create AuthService
- [ ] Create AuthController
- [ ] Configure JWT in Program.cs
- [ ] Update DataSeeder with hashed passwords
- [ ] Add [Authorize] attributes to protected endpoints
- [ ] Drop and recreate database
- [ ] Test authentication flow

Would you like me to implement all these files for you now?
