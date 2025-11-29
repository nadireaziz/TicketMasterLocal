using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<EventCategory> EventCategories { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // USER Configuration
        // ========================================
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
        });

        // ========================================
        // EVENT CATEGORY Configuration
        // ========================================
        modelBuilder.Entity<EventCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // ========================================
        // VENUE Configuration
        // ========================================
        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(e => e.VenueId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
        });

        // ========================================
        // SECTION Configuration
        // ========================================
        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.SectionId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PriceMultiplier).HasColumnType("decimal(3,2)");

            // Relationship: Section belongs to Venue
            entity.HasOne(e => e.Venue)
                  .WithMany(v => v.Sections)
                  .HasForeignKey(e => e.VenueId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================
        // SEAT Configuration
        // ========================================
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.SeatId);
            entity.Property(e => e.SeatId).HasMaxLength(50);
            entity.Property(e => e.SeatType).HasMaxLength(50);

            // Relationship: Seat belongs to Section
            entity.HasOne(e => e.Section)
                  .WithMany(s => s.Seats)
                  .HasForeignKey(e => e.SectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================
        // EVENT Configuration
        // ========================================
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.BasePrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);

            // Relationship: Event belongs to Venue
            entity.HasOne(e => e.Venue)
                  .WithMany(v => v.Events)
                  .HasForeignKey(e => e.VenueId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Event belongs to EventCategory
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Events)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // BOOKING Configuration
        // ========================================
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId);
            entity.Property(e => e.BookingReference).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.BookingReference).IsUnique();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Status).HasMaxLength(50);

            // Relationship: Booking belongs to User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Booking belongs to Event
            entity.HasOne(e => e.Event)
                  .WithMany(ev => ev.Bookings)
                  .HasForeignKey(e => e.EventId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // TICKET Configuration
        // ========================================
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.QRCode).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);

            // Relationship: Ticket belongs to Booking
            entity.HasOne(e => e.Booking)
                  .WithMany(b => b.Tickets)
                  .HasForeignKey(e => e.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Ticket references a Seat
            entity.HasOne(e => e.Seat)
                  .WithMany(s => s.Tickets)
                  .HasForeignKey(e => e.SeatId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // REFRESH TOKEN Configuration
        // ========================================
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.DeviceName).HasMaxLength(200);
            entity.Property(e => e.IpAddress).HasMaxLength(50);

            // Relationship: RefreshToken belongs to User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
