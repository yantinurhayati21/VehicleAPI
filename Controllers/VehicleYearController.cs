using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleAPI.Data;
using VehicleAPI.Models;

namespace VehicleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleYearController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VehicleYearController> _logger;

        public VehicleYearController(AppDbContext context, ILogger<VehicleYearController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/VehicleYear
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VehicleYear>>> GetVehicleYears(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? filter = null)
        {
            var query = _context.VehicleYears.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(vy => vy.Year.ToString().Contains(filter));
            }

            var total = await query.CountAsync();

            var vehicleYears = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var metadata = new
            {
                Total = total,
                Limit = limit,
                Page = page,
                NextPage = page * limit < total ? page + 1 : (int?)null,
                PrevPage = page > 1 ? page - 1 : (int?)null
            };

            return Ok(new { Message = "Vehicle years retrieved successfully.", Metadata = metadata, Years = vehicleYears });
        }

        // GET: api/VehicleYear/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<VehicleYear>> GetVehicleYear(int id)
        {
            var vehicleYear = await _context.VehicleYears.FindAsync(id);

            if (vehicleYear == null)
            {
                return NotFound(new { Message = "Vehicle year not found." });
            }

            return Ok(new { Message = "Vehicle year retrieved successfully.", Data = vehicleYear });
        }

        // POST: api/VehicleYear
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VehicleYear>> PostVehicleYear(VehicleYear vehicleYear)
        {
            try
            {
                vehicleYear.CreatedAt = DateTime.UtcNow;
                vehicleYear.UpdatedAt = DateTime.UtcNow;

                _context.VehicleYears.Add(vehicleYear);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetVehicleYear), new { id = vehicleYear.Id }, new { Message = "Vehicle year created successfully.", Data = vehicleYear });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a vehicle year.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Failed to create vehicle year." });
            }
        }

        // PATCH: api/VehicleYear/5
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PatchVehicleYear(int id, [FromBody] VehicleYear vehicleYear)
        {
            var existingYear = await _context.VehicleYears.FindAsync(id);
            if (existingYear == null)
            {
                return NotFound(new { Message = "Vehicle year not found." });
            }

            existingYear.Year = vehicleYear.Year;
            existingYear.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existingYear).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Vehicle year updated successfully." });
        }

        // DELETE: api/VehicleYear/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVehicleYear(int id)
        {
            var vehicleYear = await _context.VehicleYears.FindAsync(id);
            if (vehicleYear == null)
            {
                return NotFound(new { Message = "Vehicle year not found." });
            }

            _context.VehicleYears.Remove(vehicleYear);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Vehicle year deleted successfully." });
        }
    }
}
