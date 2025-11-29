using TicketMaster.Domain.Entities;

namespace TicketMaster.Domain.Interfaces;

/// <summary>
/// Repository pattern for Seat data access
/// </summary>
public interface ISeatRepository
{
    Task<List<Seat>> GetAllSeatsAsync();
    Task<Seat?> GetSeatByIdAsync(string seatId);
    Task UpdateSeatAsync(Seat seat);
    Task<bool> SaveChangesAsync();
}
