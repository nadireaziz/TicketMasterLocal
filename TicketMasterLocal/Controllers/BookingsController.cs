using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Infrastructure.Data;

namespace TicketMasterLocal.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // All booking endpoints require authentication
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BookingsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all bookings (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllBookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Event)
            .Include(b => b.Tickets)
            .Select(b => new
            {
                b.BookingId,
                b.BookingReference,
                b.BookingDate,
                b.TotalAmount,
                b.Status,
                User = new { b.User.FullName, b.User.Email },
                Event = new { b.Event.Name, b.Event.EventDate },
                TicketCount = b.Tickets.Count
            })
            .ToListAsync();

        return Ok(bookings);
    }

    /// <summary>
    /// Get booking details with tickets (own bookings only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingDetails(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Event)
                .ThenInclude(e => e.Venue)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Seat)
                    .ThenInclude(s => s.Section)
            .Where(b => b.BookingId == id)
            .Select(b => new
            {
                b.BookingId,
                b.UserId,
                b.BookingReference,
                b.BookingDate,
                b.TotalAmount,
                b.Status,
                User = new
                {
                    b.User.UserId,
                    b.User.FullName,
                    b.User.Email,
                    b.User.PhoneNumber
                },
                Event = new
                {
                    b.Event.EventId,
                    b.Event.Name,
                    b.Event.EventDate,
                    Venue = b.Event.Venue.Name
                },
                Tickets = b.Tickets.Select(t => new
                {
                    t.TicketId,
                    t.Price,
                    t.QRCode,
                    t.Status,
                    Seat = new
                    {
                        t.Seat.SeatId,
                        t.Seat.RowNumber,
                        t.Seat.SeatNumber,
                        Section = t.Seat.Section.Name
                    }
                })
            })
            .FirstOrDefaultAsync();

        if (booking == null)
            return NotFound();

        // Users can only see their own bookings, admins can see all
        if (booking.UserId != currentUserId && userRole != "Admin")
            return Forbid();

        return Ok(booking);
    }

    /// <summary>
    /// Get user's booking history (own bookings only, or admin can view any user)
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserBookings(int userId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Users can only see their own bookings, admins can see any user's bookings
        if (userId != currentUserId && userRole != "Admin")
            return Forbid();

        var bookings = await _context.Bookings
            .Include(b => b.Event)
            .Include(b => b.Tickets)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => new
            {
                b.BookingId,
                b.BookingReference,
                b.BookingDate,
                b.TotalAmount,
                b.Status,
                Event = new { b.Event.Name, b.Event.EventDate },
                TicketCount = b.Tickets.Count
            })
            .ToListAsync();

        return Ok(bookings);
    }

    /// <summary>
    /// Get current user's bookings (convenience endpoint)
    /// </summary>
    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings()
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var bookings = await _context.Bookings
            .Include(b => b.Event)
            .Include(b => b.Tickets)
            .Where(b => b.UserId == currentUserId)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => new
            {
                b.BookingId,
                b.BookingReference,
                b.BookingDate,
                b.TotalAmount,
                b.Status,
                Event = new { b.Event.Name, b.Event.EventDate },
                TicketCount = b.Tickets.Count
            })
            .ToListAsync();

        return Ok(bookings);
    }
}
