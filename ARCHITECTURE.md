# TicketMaster System Architecture

## Clean Architecture Implementation

This project follows **Clean Architecture** principles with clear separation of concerns across multiple layers.

## Project Structure

```
TicketMaster.sln
├── TicketMaster.Domain/          # Core business entities and interfaces
├── TicketMaster.Infrastructure/  # External concerns (DB, Redis)
├── TicketMaster.Application/     # Business logic
└── TicketMasterLocal/           # API/Presentation layer
```

## Dependency Flow

```
API Layer → Application Layer → Domain Layer ← Infrastructure Layer
```

**Key Principle:** Dependencies point INWARD. The Domain layer has NO dependencies on any other layer.

---

## 1. Domain Layer (TicketMaster.Domain)

**Purpose:** Core business entities and contracts
**Dependencies:** NONE (pure C# classes)

### Entities
- `Seat.cs` - Represents a seat in the venue

### Interfaces (Contracts)
- `ISeatRepository` - Repository pattern for seat data access
- `ICacheService` - Caching operations contract
- `IDistributedLockService` - Distributed locking contract

**Design Patterns:**
- **Repository Pattern** - Abstracts data access
- **Dependency Inversion Principle** - Interfaces defined in Domain, implemented elsewhere

---

## 2. Infrastructure Layer (TicketMaster.Infrastructure)

**Purpose:** External concerns and infrastructure implementations
**Dependencies:** Domain layer, EntityFrameworkCore, StackExchange.Redis

### Data
- `AppDbContext.cs` - Entity Framework DbContext for MySQL

### Repositories
- `SeatRepository.cs` - Implements `ISeatRepository`
  - Encapsulates all database operations for Seats
  - Uses Entity Framework for data access

### Services
- `RedisCacheService.cs` - Implements `ICacheService`
  - Provides high-performance caching using Redis
  - Serializes/deserializes objects to JSON

- `RedisDistributedLockService.cs` - Implements `IDistributedLockService`
  - Prevents race conditions using Redis SET NX EX
  - Critical for preventing double-booking

**Design Patterns:**
- **Repository Pattern** - Concrete implementation
- **Adapter Pattern** - Wraps Redis operations behind interfaces

---

## 3. Application Layer (TicketMaster.Application)

**Purpose:** Business logic and orchestration
**Dependencies:** Domain layer only

### Interfaces
- `IBookingService.cs` - Service contract for booking operations

### Services
- `BookingService.cs` - Implements `IBookingService`
  - Orchestrates repository, cache, and lock services
  - Contains business rules for seat booking
  - Implements read-through caching pattern

**Design Patterns:**
- **Service Layer Pattern** - Business logic orchestration
- **Facade Pattern** - Simplifies complex operations
- **Strategy Pattern** - Separates caching/locking strategies

---

## 4. API Layer (TicketMasterLocal)

**Purpose:** HTTP endpoints and presentation
**Dependencies:** All other layers

### Controllers
- `TicketController.cs`
  - REST API endpoints
  - Delegates to `IBookingService`

### Configuration
- `Program.cs`
  - Dependency Injection configuration
  - Middleware pipeline setup

**Design Patterns:**
- **Dependency Injection** - All dependencies injected via constructor
- **MVC Pattern** - Model-View-Controller for HTTP

---

## Design Patterns Summary

### 1. Repository Pattern
**Purpose:** Abstracts data access
**Implementation:**
```csharp
// Domain Layer - Interface
public interface ISeatRepository { }

// Infrastructure Layer - Implementation
public class SeatRepository : ISeatRepository { }

// Application Layer - Usage
public class BookingService {
    private readonly ISeatRepository _repository;
}
```

**Benefits:**
- Testability (can mock repositories)
- Flexibility (swap MySQL for PostgreSQL)
- Separation of concerns

### 2. Dependency Injection
**Purpose:** Loose coupling and testability
**Implementation:** Program.cs
```csharp
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
```

**Benefits:**
- Easy to test (inject mocks)
- Easy to swap implementations
- Follows SOLID principles

### 3. Service Layer Pattern
**Purpose:** Business logic orchestration
**Implementation:** BookingService
```csharp
public class BookingService {
    // Orchestrates multiple dependencies
    private readonly ISeatRepository _seatRepository;
    private readonly ICacheService _cacheService;
    private readonly IDistributedLockService _lockService;
}
```

**Benefits:**
- Single Responsibility
- Reusable business logic
- Testable

### 4. Clean Architecture (Onion Architecture)
**Purpose:** Dependency inversion
**Structure:**
- Domain = Core (no dependencies)
- Application = Business logic (depends on Domain)
- Infrastructure = External concerns (depends on Domain)
- API = Presentation (depends on all)

**Benefits:**
- Testable (can test business logic without database)
- Flexible (easy to change infrastructure)
- Maintainable (clear boundaries)

### 5. Read-Through Cache Pattern
**Purpose:** Performance optimization
**Flow:**
1. Check cache
2. If miss, query database
3. Update cache
4. Return data

**Implementation:** BookingService.GetSeatsAsync()

### 6. Distributed Lock Pattern
**Purpose:** Prevent race conditions in distributed systems
**Flow:**
1. Acquire lock (Redis SET NX EX)
2. Perform operation
3. Release lock

**Implementation:** BookingService.ReserveSeatAsync()

---

## Key Architectural Benefits

### 1. Testability
- Each layer can be tested independently
- Interfaces allow mocking
- No database required for unit tests

### 2. Maintainability
- Clear separation of concerns
- Each class has single responsibility
- Easy to locate code

### 3. Flexibility
- Swap MySQL for PostgreSQL? Change Infrastructure layer only
- Change business rules? Change Application layer only
- Add new API endpoints? Change API layer only

### 4. Scalability
- Distributed locking prevents race conditions
- Caching reduces database load
- Stateless design allows horizontal scaling

---

## How to Extend

### Adding a New Feature (Example: Refunds)

1. **Domain Layer:** Add `IRefundService` interface
2. **Application Layer:** Implement `RefundService`
3. **Infrastructure Layer:** Update `SeatRepository` if needed
4. **API Layer:** Add `RefundController`
5. **Program.cs:** Register services in DI

### Adding a New Data Store (Example: PostgreSQL)

1. **Infrastructure Layer:** Change connection string in Program.cs
2. **Infrastructure Layer:** Update `AppDbContext` if needed
3. No changes to Domain or Application layers!

### Adding Redis Cluster Support

1. **Infrastructure Layer:** Update Redis connection in Program.cs
2. No changes to Domain or Application layers!

---

## Testing Strategy

### Unit Tests
- **Domain Layer:** Test entities and business rules
- **Application Layer:** Test business logic with mocked dependencies
- **Infrastructure Layer:** Test repository implementations with in-memory DB

### Integration Tests
- Test API → Application → Infrastructure flow
- Use TestContainers for MySQL and Redis

### Example Unit Test
```csharp
[Fact]
public async Task BookSeat_WhenLockAcquired_ReturnsSuccess()
{
    // Arrange
    var mockRepo = new Mock<ISeatRepository>();
    var mockCache = new Mock<ICacheService>();
    var mockLock = new Mock<IDistributedLockService>();

    mockLock.Setup(x => x.AcquireLockAsync(...)).ReturnsAsync(true);

    var service = new BookingService(mockRepo.Object, mockCache.Object, mockLock.Object);

    // Act
    var result = await service.ReserveSeatAsync("A1", "UserX");

    // Assert
    Assert.StartsWith("SUCCESS", result);
}
```

---

## Conclusion

This architecture provides:
- **Separation of Concerns** - Each layer has a clear purpose
- **Testability** - Easy to unit test without dependencies
- **Maintainability** - Easy to understand and modify
- **Scalability** - Distributed locks and caching
- **Flexibility** - Easy to swap implementations

The project demonstrates production-ready patterns suitable for enterprise applications and technical interviews.
