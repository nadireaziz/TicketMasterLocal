namespace TicketMaster.Domain.Entities;

public class Seat
{
    public string SeatId { get; set; } = string.Empty; // e.g., "A1-VIP-R1-S5"
    public int SectionId { get; set; }
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public string SeatType { get; set; } = "Standard"; // Standard, Wheelchair, etc.

    // Navigation properties
    public Section Section { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
