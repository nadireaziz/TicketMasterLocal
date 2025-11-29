using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Interfaces;

/// <summary>
/// Service interface for booking operations
/// Defines the contract for seat booking business logic
/// </summary>
public interface IBookingService
{
    Task<List<Seat>> GetSeatsAsync();
    Task<string> ReserveSeatAsync(string seatId, string userId);
}
