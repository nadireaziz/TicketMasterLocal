# TicketMaster - Event Ticketing System

A scalable event ticketing platform built with .NET 8, featuring JWT authentication, real-time booking with distributed locking, and a modern web interface.

## Features

### Backend
- **Clean Architecture**: Domain, Application, Infrastructure, and API layers
- **JWT Authentication**: Access tokens (15 min) + Refresh tokens (7 days)
- **Multi-Device Sessions**: Support for up to 5 concurrent devices per user
- **Role-Based Authorization**: Customer, BoxOffice, and Admin roles
- **Distributed Locking**: Redis-based locking for concurrent booking safety
- **Caching**: Redis caching for improved performance
- **RESTful API**: Standard HTTP methods and proper status codes

### Frontend
- **Modern UI**: Gradient purple theme with smooth animations
- **Responsive Design**: Works on desktop and mobile
- **Tab Navigation**: Browse events, manage bookings, view venues
- **Real-time Updates**: Live session count and booking status
- **Token Management**: Automatic token refresh and logout

### Security
- **BCrypt Password Hashing**: Secure password storage with salt
- **Row-Level Security**: Users can only access their own bookings
- **Protected Endpoints**: Authorization required for sensitive operations
- **CORS Configuration**: Controlled cross-origin access

## Tech Stack

- **.NET 8**: Latest .NET framework
- **Entity Framework Core**: ORM with MySQL
- **MySQL**: Relational database
- **Redis**: Caching and distributed locking
- **StackExchange.Redis**: Redis client
- **JWT Bearer Authentication**: Token-based auth
- **Vanilla JavaScript**: No frontend frameworks, pure JS

## Prerequisites

- .NET 8 SDK
- MySQL Server
- Redis Server
- Docker (optional, for MySQL & Redis)

## Getting Started

### 1. Start Dependencies (Docker)

```bash
# Start MySQL
docker run -d --name mysql \
  -e MYSQL_ROOT_PASSWORD=rootpassword \
  -e MYSQL_DATABASE=ticketdb \
  -p 3306:3306 \
  mysql:latest

# Start Redis
docker run -d --name redis \
  -p 6379:6379 \
  redis:latest
```

### 2. Run the Application

```bash
dotnet restore
dotnet run
```

The application will be available at: http://localhost:5277

### 3. Access the UI

- **Login Page**: http://localhost:5277/
- **Dashboard**: http://localhost:5277/dashboard (after login)

### Test Credentials

**Customer Account:**
- Email: `john.doe@email.com`
- Password: `Password123!`

**Admin Account:**
- Email: `admin@ticketmaster.com`
- Password: `Admin123!`

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get tokens
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout and revoke tokens
- `GET /api/auth/me` - Get current user info (protected)
- `GET /api/auth/sessions` - Get active session count (protected)

### Events
- `GET /api/events` - List all events (public)
- `GET /api/events/{id}` - Get event details (public)

### Venues
- `GET /api/venues` - List all venues (public)
- `GET /api/venues/{id}` - Get venue details (public)

### Bookings
- `GET /api/bookings` - List all bookings (admin only)
- `GET /api/bookings/{id}` - Get booking details (protected)
- `GET /api/bookings/my-bookings` - Get current user's bookings (protected)
- `POST /api/bookings` - Create new booking (protected)

### Tickets
- `POST /api/tickets/book` - Book tickets with distributed locking (protected)

## Project Structure

```
TicketMasterLocal/
├── Controllers/          # API controllers
│   ├── AuthController.cs
│   ├── BookingsController.cs
│   ├── EventsController.cs
│   ├── TicketController.cs
│   └── VenuesController.cs
├── DTOs/                 # Data transfer objects
│   └── Auth/
├── wwwroot/             # Static files (frontend)
│   ├── index.html       # Login/registration page
│   ├── auth.css         # Login page styles
│   ├── auth.js          # Login page logic
│   ├── app-dashboard.html
│   ├── dashboard.css
│   └── dashboard-app.js
├── Program.cs           # Application entry point
└── appsettings.json     # Configuration

TicketMaster.Domain/     # Domain entities (referenced)
TicketMaster.Application/ # Business logic (referenced)
TicketMaster.Infrastructure/ # Data access (referenced)
```

## Configuration

Edit `appsettings.json` for custom settings:

```json
{
  "Jwt": {
    "Secret": "your-secret-key-min-32-characters-long",
    "Issuer": "TicketMaster",
    "Audience": "TicketMasterApp"
  }
}
```

## Development

### Database Migrations

The application uses `EnsureCreated()` for development. For production:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

## License

MIT License
