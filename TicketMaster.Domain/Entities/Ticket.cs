namespace TicketMaster.Domain.Entities;

public class Ticket
{
    public int TicketId { get; set; }
    public int BookingId { get; set; }
    public string SeatId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string QRCode { get; set; } = string.Empty; // QR code for validation
    public string Status { get; set; } = "Valid"; // Valid, Used, Cancelled

    // Navigation properties
    public Booking Booking { get; set; } = null!;
    public Seat Seat { get; set; } = null!;
}
