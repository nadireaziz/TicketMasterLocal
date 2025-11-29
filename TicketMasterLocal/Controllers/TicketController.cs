using Microsoft.AspNetCore.Mvc;
using TicketMaster.Application.Interfaces;

namespace TicketMasterLocal.Controllers;

/// <summary>
/// API Controller for ticket booking operations
/// Uses Dependency Injection to access business logic
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public TicketController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Get all available seats
    /// Uses read-through caching for performance
    /// </summary>
    [HttpGet("seats")]
    public async Task<IActionResult> GetSeats()
    {
        var seats = await _bookingService.GetSeatsAsync();
        return Ok(seats);
    }

    /// <summary>
    /// Book a specific seat for a user
    /// Uses distributed locking to prevent double-booking
    /// </summary>
    [HttpPost("book")]
    public async Task<IActionResult> BookSeat([FromQuery] string seatId, [FromQuery] string userId)
    {
        var result = await _bookingService.ReserveSeatAsync(seatId, userId);

        if (result.StartsWith("FAILURE") || result.StartsWith("Error"))
        {
            return Conflict(result);
        }

        return Ok(result);
    }
}
