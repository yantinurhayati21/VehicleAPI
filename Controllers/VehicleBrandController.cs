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
    public class VehicleBrandController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehicleBrandController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/VehicleBrand
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleBrands(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? filter = "")
        {
            var query = _context.VehicleBrands.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(b => b.Name.Contains(filter));
            }

            var total = await query.CountAsync();
            var brands = await query
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

            return Ok(new { Metadata = metadata, Brands = brands });
        }

        // GET: api/VehicleBrand/3
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleBrand(int id)
        {
            var vehicleBrand = await _context.VehicleBrands.FindAsync(id);
            if (vehicleBrand == null)
            {
                return NotFound(new { Message = "Brand not found" });
            }

            return Ok(vehicleBrand);
        }

        // POST: api/VehicleBrand
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VehicleBrand>> PostVehicleBrand(VehicleBrand vehicleBrand)
        {
            _context.VehicleBrands.Add(vehicleBrand);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVehicleBrand), new { id = vehicleBrand.Id }, new { Message = "Brand added successfully", Data = vehicleBrand });
        }

        // PATCH: api/VehicleBrand/3
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PatchVehicleBrand(int id, [FromBody] VehicleBrand vehicleBrand)
        {
            var existingBrand = await _context.VehicleBrands.FindAsync(id);
            if (existingBrand == null)
            {
                return NotFound(new { Message = "Brand not found" });
            }

            existingBrand.Name = vehicleBrand.Name ?? existingBrand.Name;
            existingBrand.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Brand updated successfully", Data = existingBrand });
        }

        // DELETE: api/VehicleBrand/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVehicleBrand(int id)
        {
            var vehicleBrand = await _context.VehicleBrands.FindAsync(id);
            if (vehicleBrand == null)
            {
                return NotFound(new { Message = "Brand not found" });
            }

            _context.VehicleBrands.Remove(vehicleBrand);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Brand deleted successfully" });
        }
    }
}
