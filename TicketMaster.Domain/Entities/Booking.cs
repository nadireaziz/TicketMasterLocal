namespace TicketMaster.Domain.Entities;

public class Booking
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
    public string BookingReference { get; set; } = string.Empty; // Unique code like "BK-2025-ABC123"

    // Navigation properties
    public User User { get; set; } = null!;
    public Event Event { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
