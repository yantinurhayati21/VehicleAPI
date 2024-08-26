using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleAPI.Data;
using VehicleAPI.Models;

namespace VehicleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleTypeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehicleTypeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/VehicleType
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VehicleType>>> GetVehicleTypes(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] int brandId = 0)
        {
            var query = _context.VehicleTypes.AsQueryable();

            if (brandId > 0)
            {
                query = query.Where(vt => vt.BrandId == brandId);
            }

            var total = await query.CountAsync();

            var vehicleTypes = await query
                .Include(vt => vt.Brand)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var metadata = new
            {
                TotalItems = total,
                Limit = limit,
                Page = page,
                TotalPages = (int)Math.Ceiling((double)total / limit),
                NextPage = page * limit < total ? (int?)page + 1 : null,
                PrevPage = page > 1 ? (int?)page - 1 : null
            };

            return Ok(new { Metadata = metadata, Types = vehicleTypes });
        }

        // GET: api/VehicleType/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<VehicleType>> GetVehicleType(int id)
        {
            var vehicleType = await _context.VehicleTypes
                .Include(vt => vt.Brand)
                .SingleOrDefaultAsync(vt => vt.Id == id);

            if (vehicleType == null)
            {
                return NotFound(new { Message = "Vehicle type not found." });
            }

            return Ok(vehicleType);
        }

        // POST: api/VehicleType
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VehicleType>> PostVehicleType(VehicleType vehicleType)
        {
            vehicleType.CreatedAt = DateTime.UtcNow;
            vehicleType.UpdatedAt = DateTime.UtcNow;

            if (vehicleType.BrandId != 0)
            {
                var brand = await _context.VehicleBrands.FindAsync(vehicleType.BrandId);
                if (brand == null)
                {
                    return BadRequest(new { Message = "Invalid BrandId provided." });
                }
                vehicleType.Brand = brand;
            }

            _context.VehicleTypes.Add(vehicleType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVehicleType), new { id = vehicleType.Id }, new { Message = "Vehicle type added successfully.", Data = vehicleType });
        }

        // PATCH: api/VehicleType/5
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PatchVehicleType(int id, [FromBody] VehicleType vehicleType)
        {
            var existingType = await _context.VehicleTypes.FindAsync(id);
            if (existingType == null)
            {
                return NotFound(new { Message = "Vehicle type not found." });
            }

            existingType.Name = vehicleType.Name ?? existingType.Name;
            existingType.BrandId = vehicleType.BrandId != 0 ? vehicleType.BrandId : existingType.BrandId;
            existingType.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Vehicle type updated successfully.", Data = existingType });
        }

        // DELETE: api/VehicleType/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVehicleType(int id)
        {
            var vehicleType = await _context.VehicleTypes.FindAsync(id);
            if (vehicleType == null)
            {
                return NotFound(new { Message = "Vehicle type not found." });
            }

            _context.VehicleTypes.Remove(vehicleType);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Vehicle type deleted successfully." });
        }
    }
}
