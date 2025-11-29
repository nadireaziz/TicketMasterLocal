using TicketMaster.Domain.Entities;

namespace TicketMaster.Infrastructure.Data;

public static class DataSeeder
{
    public static void SeedData(AppDbContext context)
    {
        // Check if data already exists
        if (context.Users.Any() || context.Events.Any())
        {
            return; // Database already seeded
        }

        Console.WriteLine("Seeding database with sample data...");

        // ===== SEED USERS =====
        var users = new List<User>
        {
            new User
            {
                Email = "john.doe@email.com",
                FullName = "John Doe",
                PhoneNumber = "+1-555-0101",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Email = "jane.smith@email.com",
                FullName = "Jane Smith",
                PhoneNumber = "+1-555-0102",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Email = "admin@ticketmaster.com",
                FullName = "Admin User",
                PhoneNumber = "+1-555-9999",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        // ===== SEED EVENT CATEGORIES =====
        var categories = new List<EventCategory>
        {
            new EventCategory { Name = "Concert", Description = "Live music performances" },
            new EventCategory { Name = "Sports", Description = "Sporting events and games" },
            new EventCategory { Name = "Theater", Description = "Plays, musicals, and performances" },
            new EventCategory { Name = "Comedy", Description = "Stand-up and comedy shows" }
        };
        context.EventCategories.AddRange(categories);
        context.SaveChanges();

        // ===== SEED VENUES =====
        var venues = new List<Venue>
        {
            new Venue
            {
                Name = "Madison Square Garden",
                Address = "4 Pennsylvania Plaza",
                City = "New York",
                Capacity = 20000,
                ImageUrl = "https://example.com/msg.jpg"
            },
            new Venue
            {
                Name = "Staples Center",
                Address = "1111 S Figueroa St",
                City = "Los Angeles",
                Capacity = 19000,
                ImageUrl = "https://example.com/staples.jpg"
            }
        };
        context.Venues.AddRange(venues);
        context.SaveChanges();

        // ===== SEED SECTIONS =====
        var msg = venues[0]; // Madison Square Garden
        var staples = venues[1]; // Staples Center

        var sections = new List<Section>
        {
            // Madison Square Garden Sections
            new Section { VenueId = msg.VenueId, Name = "VIP", PriceMultiplier = 2.0m, RowCount = 5, SeatsPerRow = 20 },
            new Section { VenueId = msg.VenueId, Name = "Premium", PriceMultiplier = 1.5m, RowCount = 10, SeatsPerRow = 25 },
            new Section { VenueId = msg.VenueId, Name = "General", PriceMultiplier = 1.0m, RowCount = 20, SeatsPerRow = 30 },
            // Staples Center Sections
            new Section { VenueId = staples.VenueId, Name = "VIP", PriceMultiplier = 2.0m, RowCount = 5, SeatsPerRow = 20 },
            new Section { VenueId = staples.VenueId, Name = "Premium", PriceMultiplier = 1.5m, RowCount = 10, SeatsPerRow = 25 },
            new Section { VenueId = staples.VenueId, Name = "General", PriceMultiplier = 1.0m, RowCount = 20, SeatsPerRow = 30 }
        };
        context.Sections.AddRange(sections);
        context.SaveChanges();

        // ===== SEED SEATS =====
        var seats = new List<Seat>();
        foreach (var section in sections)
        {
            for (int row = 1; row <= section.RowCount; row++)
            {
                for (int seatNum = 1; seatNum <= section.SeatsPerRow; seatNum++)
                {
                    seats.Add(new Seat
                    {
                        SeatId = $"V{section.VenueId}-{section.Name}-R{row}-S{seatNum}",
                        SectionId = section.SectionId,
                        RowNumber = row,
                        SeatNumber = seatNum,
                        SeatType = "Standard"
                    });
                }
            }
        }
        context.Seats.AddRange(seats);
        context.SaveChanges();

        // ===== SEED EVENTS =====
        var events = new List<Event>
        {
            new Event
            {
                VenueId = msg.VenueId,
                CategoryId = categories[0].CategoryId, // Concert
                Name = "Taylor Swift - Eras Tour",
                Description = "The biggest pop concert of the decade!",
                EventDate = DateTime.UtcNow.AddDays(30),
                BasePrice = 150.00m,
                Status = "Upcoming",
                ImageUrl = "https://example.com/taylor.jpg"
            },
            new Event
            {
                VenueId = staples.VenueId,
                CategoryId = categories[1].CategoryId, // Sports
                Name = "Lakers vs Warriors",
                Description = "NBA Finals Game 7",
                EventDate = DateTime.UtcNow.AddDays(15),
                BasePrice = 80.00m,
                Status = "Upcoming",
                ImageUrl = "https://example.com/lakers.jpg"
            },
            new Event
            {
                VenueId = msg.VenueId,
                CategoryId = categories[2].CategoryId, // Theater
                Name = "Hamilton",
                Description = "The award-winning Broadway musical",
                EventDate = DateTime.UtcNow.AddDays(45),
                BasePrice = 120.00m,
                Status = "Upcoming",
                ImageUrl = "https://example.com/hamilton.jpg"
            },
            new Event
            {
                VenueId = staples.VenueId,
                CategoryId = categories[3].CategoryId, // Comedy
                Name = "Dave Chappelle Live",
                Description = "Stand-up comedy special",
                EventDate = DateTime.UtcNow.AddDays(60),
                BasePrice = 60.00m,
                Status = "Upcoming",
                ImageUrl = "https://example.com/chappelle.jpg"
            }
        };
        context.Events.AddRange(events);
        context.SaveChanges();

        // ===== SEED SAMPLE BOOKING =====
        var sampleBooking = new Booking
        {
            UserId = users[0].UserId,
            EventId = events[0].EventId,
            BookingDate = DateTime.UtcNow,
            TotalAmount = 300.00m,
            Status = "Confirmed",
            BookingReference = "BK-2025-ABC123"
        };
        context.Bookings.Add(sampleBooking);
        context.SaveChanges();

        // ===== SEED SAMPLE TICKETS =====
        var vipSeats = seats.Where(s => s.SeatId.Contains("VIP")).Take(2).ToList();
        var tickets = vipSeats.Select(seat => new Ticket
        {
            BookingId = sampleBooking.BookingId,
            SeatId = seat.SeatId,
            Price = 150.00m,
            QRCode = Guid.NewGuid().ToString(),
            Status = "Valid"
        }).ToList();
        context.Tickets.AddRange(tickets);
        context.SaveChanges();

        Console.WriteLine("✅ Database seeded successfully!");
        Console.WriteLine($"   - {users.Count} Users");
        Console.WriteLine($"   - {categories.Count} Event Categories");
        Console.WriteLine($"   - {venues.Count} Venues");
        Console.WriteLine($"   - {sections.Count} Sections");
        Console.WriteLine($"   - {seats.Count} Seats");
        Console.WriteLine($"   - {events.Count} Events");
        Console.WriteLine($"   - 1 Sample Booking with 2 Tickets");
    }
}
