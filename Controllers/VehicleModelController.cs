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
    public class VehicleModelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehicleModelController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/VehicleModel
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleModels(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? filter = null)
        {
            // Using LINQ to build the query with filtering and pagination in a single step
            var models = await _context.VehicleModels
                .Where(vm => string.IsNullOrEmpty(filter) || vm.Name.Contains(filter))
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var total = await _context.VehicleModels.CountAsync(vm => string.IsNullOrEmpty(filter) || vm.Name.Contains(filter));

            // Returning metadata and models in a structured format
            return Ok(new
            {
                Metadata = new
                {
                    Total = total,
                    Limit = limit,
                    Page = page,
                    HasNextPage = page * limit < total,
                    HasPreviousPage = page > 1
                },
                Models = models
            });
        }

        // GET: api/VehicleModel/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleModel(int id)
        {
            var vehicleModel = await _context.VehicleModels.FindAsync(id);

            if (vehicleModel == null)
            {
                return NotFound(new { message = "Vehicle model not found." });
            }

            return Ok(vehicleModel);
        }

        // POST: api/VehicleModel
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostVehicleModel([FromBody] VehicleModel vehicleModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _context.VehicleTypes.AnyAsync(vt => vt.Id == vehicleModel.TypeId))
            {
                return BadRequest(new { message = "Invalid TypeId provided." });
            }

            vehicleModel.CreatedAt = DateTime.UtcNow;
            vehicleModel.UpdatedAt = DateTime.UtcNow;

            await _context.VehicleModels.AddAsync(vehicleModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVehicleModel), new { id = vehicleModel.Id }, 
                new { message = "Vehicle model successfully added.", vehicleModel });
        }

        // PATCH: api/VehicleModel/5
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PatchVehicleModel(int id, [FromBody] VehicleModel updatedModel)
        {
            var vehicleModel = await _context.VehicleModels.FindAsync(id);
            if (vehicleModel == null)
            {
                return NotFound(new { message = "Vehicle model not found." });
            }

            if (!await _context.VehicleTypes.AnyAsync(vt => vt.Id == updatedModel.TypeId))
            {
                return BadRequest(new { message = "Invalid TypeId provided." });
            }

            // Updating only the necessary fields
            vehicleModel.Name = updatedModel.Name;
            vehicleModel.TypeId = updatedModel.TypeId;
            vehicleModel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Vehicle model successfully updated.", vehicleModel });
        }

        // DELETE: api/VehicleModel/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVehicleModel(int id)
        {
            var vehicleModel = await _context.VehicleModels.FindAsync(id);
            if (vehicleModel == null)
            {
                return NotFound(new { message = "Vehicle model not found." });
            }

            _context.VehicleModels.Remove(vehicleModel);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vehicle model successfully deleted." });
        }
    }
}
