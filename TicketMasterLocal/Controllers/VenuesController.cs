using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Infrastructure.Data;

namespace TicketMasterLocal.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly AppDbContext _context;

    public VenuesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all venues
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllVenues()
    {
        var venues = await _context.Venues
            .Select(v => new
            {
                v.VenueId,
                v.Name,
                v.City,
                v.Capacity,
                v.ImageUrl,
                EventCount = v.Events.Count(),
                SectionCount = v.Sections.Count()
            })
            .ToListAsync();

        return Ok(venues);
    }

    /// <summary>
    /// Get venue seating layout
    /// </summary>
    [HttpGet("{id}/seating")]
    public async Task<IActionResult> GetVenueSeating(int id)
    {
        var venue = await _context.Venues
            .Include(v => v.Sections)
                .ThenInclude(s => s.Seats)
            .Where(v => v.VenueId == id)
            .Select(v => new
            {
                v.VenueId,
                v.Name,
                Sections = v.Sections.Select(s => new
                {
                    s.SectionId,
                    s.Name,
                    s.PriceMultiplier,
                    s.RowCount,
                    s.SeatsPerRow,
                    TotalSeats = s.Seats.Count()
                })
            })
            .FirstOrDefaultAsync();

        if (venue == null)
            return NotFound();

        return Ok(venue);
    }
}
