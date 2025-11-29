using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Services;
using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Interfaces;
using TicketMaster.Infrastructure.Data;
using TicketMaster.Infrastructure.Repositories;
using TicketMaster.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// DEPENDENCY INJECTION CONFIGURATION
// Clean Architecture Pattern
// ========================================

// 1. Infrastructure Layer - Database
var connectionString = "server=localhost;user=root;password=rootpassword;database=ticketdb";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. Infrastructure Layer - Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false"));

// 3. Infrastructure Layer - Repository Pattern
builder.Services.AddScoped<ISeatRepository, SeatRepository>();

// 4. Infrastructure Layer - Service Implementations
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IDistributedLockService, RedisDistributedLockService>();

// 5. Application Layer - Business Logic
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 6. API Layer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 7. CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 8. JWT Authentication & Authorization
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

var app = builder.Build();

// ========================================
// DATABASE SEEDING
// ========================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DataSeeder.SeedData(db);
}

// ========================================
// MIDDLEWARE PIPELINE
// ========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();

// Serve login page at root
app.MapGet("/", () => Results.Redirect("/index.html"));

// Redirect /dashboard to user dashboard
app.MapGet("/dashboard", () => Results.Redirect("/app-dashboard.html"));

app.Run();
