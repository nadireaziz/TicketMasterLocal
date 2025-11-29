namespace TicketMaster.Domain.Entities;

public class Event
{
    public int EventId { get; set; }
    public int VenueId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = "Upcoming"; // Upcoming, Ongoing, Completed, Cancelled
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Venue Venue { get; set; } = null!;
    public EventCategory Category { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
