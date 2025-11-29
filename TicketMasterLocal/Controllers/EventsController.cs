using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Infrastructure.Data;

namespace TicketMasterLocal.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all events with venue and category details
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Category)
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                e.EventId,
                e.Name,
                e.Description,
                e.EventDate,
                e.BasePrice,
                e.Status,
                e.ImageUrl,
                Venue = new { e.Venue.VenueId, e.Venue.Name, e.Venue.City },
                Category = new { e.Category.CategoryId, e.Category.Name }
            })
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>
    /// Get events by category
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetEventsByCategory(int categoryId)
    {
        var events = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Category)
            .Where(e => e.CategoryId == categoryId)
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>
    /// Get event details with available seats
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventDetails(int id)
    {
        var eventDetails = await _context.Events
            .Include(e => e.Venue)
                .ThenInclude(v => v.Sections)
                    .ThenInclude(s => s.Seats)
            .Include(e => e.Category)
            .Where(e => e.EventId == id)
            .Select(e => new
            {
                e.EventId,
                e.Name,
                e.Description,
                e.EventDate,
                e.BasePrice,
                e.Status,
                Venue = new
                {
                    e.Venue.VenueId,
                    e.Venue.Name,
                    e.Venue.City,
                    e.Venue.Capacity,
                    Sections = e.Venue.Sections.Select(s => new
                    {
                        s.SectionId,
                        s.Name,
                        s.PriceMultiplier,
                        Price = e.BasePrice * s.PriceMultiplier,
                        AvailableSeats = s.Seats.Count(seat => !_context.Tickets
                            .Any(t => t.SeatId == seat.SeatId && t.Booking.EventId == e.EventId))
                    })
                },
                Category = e.Category.Name
            })
            .FirstOrDefaultAsync();

        if (eventDetails == null)
            return NotFound();

        return Ok(eventDetails);
    }
}
