namespace TicketMaster.Domain.Entities;

public class Section
{
    public int SectionId { get; set; }
    public int VenueId { get; set; }
    public string Name { get; set; } = string.Empty; // VIP, Premium, General
    public decimal PriceMultiplier { get; set; } = 1.0m; // 2.0 for VIP, 1.5 for Premium, 1.0 for General
    public int RowCount { get; set; }
    public int SeatsPerRow { get; set; }

    // Navigation properties
    public Venue Venue { get; set; } = null!;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
