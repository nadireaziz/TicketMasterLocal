using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Interfaces;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Infrastructure.Repositories;

/// <summary>
/// Repository pattern implementation for Seat entity
/// Encapsulates data access logic
/// </summary>
public class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _context;

    public SeatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Seat>> GetAllSeatsAsync()
    {
        return await _context.Seats.ToListAsync();
    }

    public async Task<Seat?> GetSeatByIdAsync(string seatId)
    {
        return await _context.Seats.FindAsync(seatId);
    }

    public async Task UpdateSeatAsync(Seat seat)
    {
        _context.Seats.Update(seat);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
