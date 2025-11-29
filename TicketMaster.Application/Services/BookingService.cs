using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Interfaces;

namespace TicketMaster.Application.Services;

/// <summary>
/// Service Layer Pattern - Business logic for ticket booking
/// Note: This is the legacy booking service for backward compatibility
/// For new event-based booking, use the EventsController directly
/// </summary>
public class BookingService : IBookingService
{
    private readonly ISeatRepository _seatRepository;
    private readonly ICacheService _cacheService;
    private readonly IDistributedLockService _lockService;

    private const string CACHE_KEY_ALL_SEATS = "all_seats";
    private const int CACHE_EXPIRATION_SECONDS = 10;

    public BookingService(
        ISeatRepository seatRepository,
        ICacheService cacheService,
        IDistributedLockService lockService)
    {
        _seatRepository = seatRepository;
        _cacheService = cacheService;
        _lockService = lockService;
    }

    public async Task<List<Seat>> GetSeatsAsync()
    {
        // Try cache first
        var cachedSeats = await _cacheService.GetAsync<List<Seat>>(CACHE_KEY_ALL_SEATS);
        if (cachedSeats != null)
        {
            return cachedSeats;
        }

        // Fallback to database
        var seats = await _seatRepository.GetAllSeatsAsync();

        // Set cache for next request
        await _cacheService.SetAsync(
            CACHE_KEY_ALL_SEATS,
            seats,
            TimeSpan.FromSeconds(CACHE_EXPIRATION_SECONDS));

        return seats;
    }

    public async Task<string> ReserveSeatAsync(string seatId, string userId)
    {
        // This is a legacy method - kept for backward compatibility
        // For new bookings, use the booking workflow through EventsController
        return "Please use the /api/events endpoint for booking tickets with the new system.";
    }
}
