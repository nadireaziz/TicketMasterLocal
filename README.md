# TicketMaster - Event Ticketing System

A scalable, production-ready event ticketing platform built with .NET 8 Clean Architecture, featuring JWT authentication, distributed locking, and real-time booking capabilities.

## Architecture

This solution follows **Clean Architecture** principles with clear separation of concerns:

```
TicketMaster/
├── TicketMaster.Domain/          # Core business entities
│   ├── Entities/                 # User, Event, Booking, Ticket, etc.
│   └── Interfaces/               # Repository contracts
│
├── TicketMaster.Application/     # Business logic layer
│   ├── Services/                 # IAuthService, IBookingService
│   └── Interfaces/               # Service contracts
│
├── TicketMaster.Infrastructure/  # Data access & external services
│   ├── Data/                     # EF Core DbContext
│   ├── Repositories/             # Repository implementations
│   └── Services/                 # Redis cache, distributed locking
│
└── TicketMasterLocal/           # API & Presentation layer
    ├── Controllers/              # REST API endpoints
    ├── DTOs/                     # Data transfer objects
    └── wwwroot/                  # Frontend UI
```

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture documentation.

## Features

### Backend
- **JWT Authentication**: Stateless auth with access tokens (15 min) and refresh tokens (7 days)
- **Multi-Device Sessions**: Support up to 5 concurrent devices per user with automatic oldest session revocation
- **Role-Based Authorization**: Customer, BoxOffice, and Admin roles with granular permissions
- **Distributed Locking**: Redis-based locking prevents race conditions in concurrent bookings
- **Caching**: Redis caching for improved read performance
- **Row-Level Security**: Users can only access their own bookings; admins can access all
- **Clean Architecture**: Testable, maintainable, and scalable design
- **Repository Pattern**: Clean data access abstraction

### Frontend
- **Modern UI**: Gradient purple theme (#667eea → #764ba2) with smooth animations
- **Responsive Design**: Mobile-first design that works on all devices
- **SPA-like Experience**: Tab navigation without page reloads
- **Real-time Updates**: Live session count and booking status
- **Token Management**: Automatic localStorage handling with refresh flow
- **Vanilla JavaScript**: No frameworks, pure JS for maximum performance

### Security
- **BCrypt Password Hashing**: Secure password storage with salt rounds
- **JWT Token Validation**: Issuer, audience, lifetime, and signature validation
- **CORS Configuration**: Controlled cross-origin access
- **Protected Endpoints**: Authorization required for sensitive operations
- **SQL Injection Protection**: Parameterized queries via EF Core

## Tech Stack

| Layer | Technologies |
|-------|-------------|
| **Backend** | .NET 8, ASP.NET Core Web API |
| **ORM** | Entity Framework Core 8 |
| **Database** | MySQL 8.0 |
| **Caching** | Redis 7, StackExchange.Redis |
| **Authentication** | JWT Bearer, BCrypt.Net |
| **Frontend** | HTML5, CSS3, Vanilla JavaScript |
| **Containerization** | Docker, Docker Compose |

## Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **MySQL 8.0** - Database server
- **Redis 7** - Caching and distributed locking
- **Docker** (optional) - For containerized MySQL & Redis

## Quick Start

### 1. Start Dependencies with Docker

```bash
# Navigate to solution directory
cd "/path/to/system design"

# Start MySQL and Redis
docker-compose up -d

# Verify services are running
docker ps
```

### 2. Restore and Run

```bash
# Restore NuGet packages
dotnet restore

# Run the application
cd TicketMasterLocal
dotnet run
```

The application will be available at:
- **API**: http://localhost:5277
- **Swagger**: http://localhost:5277/swagger
- **UI**: http://localhost:5277/

### 3. Test the Application

**Option 1: Use the Web UI**
- Open http://localhost:5277/
- Click "Create Account" and register
- Login and explore the dashboard

**Option 2: Use Test Credentials**
- **Customer**: `john.doe@email.com` / `Password123!`
- **Admin**: `admin@ticketmaster.com` / `Admin123!`

## Project Structure

```
system design/
├── docker-compose.yml              # MySQL & Redis services
├── TicketMaster.sln                # Solution file
├── ARCHITECTURE.md                 # Architecture documentation
├── AUTHENTICATION_IMPLEMENTATION.md # Auth details
│
├── TicketMaster.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Event.cs
│   │   ├── Booking.cs
│   │   ├── Ticket.cs
│   │   ├── Venue.cs
│   │   └── RefreshToken.cs
│   └── Interfaces/
│       └── ISeatRepository.cs
│
├── TicketMaster.Application/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   └── IBookingService.cs
│   └── Services/
│       ├── AuthService.cs          # Authentication logic
│       └── BookingService.cs       # Booking with distributed locks
│
├── TicketMaster.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs         # EF Core context
│   │   └── DataSeeder.cs           # Seed data
│   ├── Repositories/
│   │   └── SeatRepository.cs
│   └── Services/
│       ├── RedisCacheService.cs
│       └── RedisDistributedLockService.cs
│
└── TicketMasterLocal/              # API & UI
    ├── Controllers/
    │   ├── AuthController.cs       # /api/auth/*
    │   ├── BookingsController.cs   # /api/bookings/*
    │   ├── EventsController.cs     # /api/events/*
    │   ├── VenuesController.cs     # /api/venues/*
    │   └── TicketController.cs     # /api/tickets/*
    ├── DTOs/Auth/
    │   ├── LoginRequest.cs
    │   ├── RegisterRequest.cs
    │   ├── TokenResponse.cs
    │   └── RefreshTokenRequest.cs
    ├── wwwroot/                    # Frontend
    │   ├── index.html              # Login/registration
    │   ├── auth.css
    │   ├── auth.js
    │   ├── app-dashboard.html      # User dashboard
    │   ├── dashboard.css
    │   └── dashboard-app.js
    ├── Program.cs                  # Application entry point
    └── appsettings.json
```

## API Endpoints

### Authentication (`/api/auth`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/register` | Public | Register new user |
| POST | `/login` | Public | Login and get JWT tokens |
| POST | `/refresh` | Public | Refresh access token |
| POST | `/logout` | Protected | Logout and revoke tokens |
| GET | `/me` | Protected | Get current user info |
| GET | `/sessions` | Protected | Get active session count |

### Events (`/api/events`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | Public | List all events |
| GET | `/{id}` | Public | Get event details |

### Venues (`/api/venues`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | Public | List all venues |
| GET | `/{id}` | Public | Get venue details |

### Bookings (`/api/bookings`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | Admin | List all bookings |
| GET | `/{id}` | Protected | Get booking details (own or admin) |
| GET | `/my-bookings` | Protected | Get current user's bookings |
| POST | `/` | Protected | Create new booking |

### Tickets (`/api/tickets`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/book` | Protected | Book tickets with distributed locking |

## Configuration

### Database Connection
Edit `TicketMasterLocal/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;user=root;password=rootpassword;database=ticketdb"
  }
}
```

### JWT Settings
```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-characters-long-for-production",
    "Issuer": "TicketMaster",
    "Audience": "TicketMasterApp"
  }
}
```

### Redis Configuration
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379,abortConnect=false"
  }
}
```

## Development

### Build the Solution
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Database Migrations
```bash
# Create migration
dotnet ef migrations add MigrationName --project TicketMaster.Infrastructure --startup-project TicketMasterLocal

# Apply migration
dotnet ef database update --project TicketMaster.Infrastructure --startup-project TicketMasterLocal
```

### Run with Watch Mode
```bash
cd TicketMasterLocal
dotnet watch run
```

## Docker Deployment

### Using Docker Compose
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

## Production Deployment

1. **Update Configuration**
   - Use strong JWT secret (min 32 characters)
   - Update database connection strings
   - Configure CORS for your domain
   - Enable HTTPS

2. **Publish Application**
```bash
dotnet publish -c Release -o ./publish
```

3. **Environment Variables**
```bash
export ASPNETCORE_ENVIRONMENT=Production
export JWT_SECRET="your-production-secret"
export DB_CONNECTION="your-production-db"
```

## Security Considerations

- **Passwords**: Hashed with BCrypt (cost factor 12)
- **Tokens**: JWT with HMAC-SHA256 signing
- **Session Management**: Max 5 devices with automatic cleanup
- **CORS**: Configure allowed origins in production
- **HTTPS**: Always use HTTPS in production
- **Secrets**: Never commit secrets to git

## Performance Optimizations

- Redis caching for frequently accessed data
- Distributed locking prevents database contention
- Connection pooling for database and Redis
- Indexed queries via EF Core
- Stateless JWT reduces database lookups

## Troubleshooting

**MySQL Connection Issues:**
```bash
# Check MySQL is running
docker ps | grep mysql

# Restart MySQL
docker-compose restart mysql
```

**Redis Connection Issues:**
```bash
# Check Redis is running
docker ps | grep redis

# Test connection
redis-cli ping
```

**Port Already in Use:**
```bash
# Find process using port 5277
lsof -i :5277

# Kill process
kill -9 <PID>
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

MIT License - See LICENSE file for details

## Acknowledgments

- Built with Clean Architecture principles
- Inspired by domain-driven design patterns
- Frontend design uses modern CSS gradients and animations
