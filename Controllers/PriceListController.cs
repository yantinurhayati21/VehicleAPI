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
    public class PriceListController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PriceListController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PriceList
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PriceList>>> GetPriceLists(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] int? yearId = null,
            [FromQuery] int? modelId = null)
        {
            var query = _context.PriceLists.AsQueryable();

            if (yearId.HasValue)
            {
                query = query.Where(p => p.YearId == yearId.Value);
            }

            if (modelId.HasValue)
            {
                query = query.Where(p => p.ModelId == modelId.Value);
            }

            var total = await query.CountAsync();

            var priceLists = await query
                .Include(p => p.Year)
                .Include(p => p.Model)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var metadata = new
            {
                Total = total,
                Limit = limit,
                Page = page,
                NextPage = (page * limit < total) ? page + 1 : (int?)null,
                PrevPage = (page > 1) ? page - 1 : (int?)null
            };

            return Ok(new { Message = "Price lists retrieved successfully.", Metadata = metadata, PriceLists = priceLists });
        }

        // GET: api/PriceList/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PriceList>> GetPriceList(int id)
        {
            var priceList = await _context.PriceLists
                .Include(p => p.Year)
                .Include(p => p.Model)
                .SingleOrDefaultAsync(p => p.Id == id);

            if (priceList == null)
            {
                return NotFound(new { Message = "Price list not found." });
            }

            return Ok(new { Message = "Price list retrieved successfully.", Data = priceList });
        }

        // POST: api/PriceList
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PriceList>> PostPriceList(PriceList priceList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid price list data.", Errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            _context.PriceLists.Add(priceList);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPriceList), new { id = priceList.Id }, new { Message = "Price list created successfully.", Data = priceList });
        }

        // PATCH: api/PriceList/5
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PatchPriceList(int id, [FromBody] PriceList priceList)
        {

            var existingPriceList = await _context.PriceLists.FindAsync(id);
            if (existingPriceList == null)
            {
                return NotFound(new { Message = "Price list not found." });
            }

            existingPriceList.Code = priceList.Code;
            existingPriceList.Price = priceList.Price;
            existingPriceList.YearId = priceList.YearId;
            existingPriceList.ModelId = priceList.ModelId;
            existingPriceList.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existingPriceList).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Price list updated successfully." });
        }

        // DELETE: api/PriceList/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePriceList(int id)
        {
            var priceList = await _context.PriceLists.FindAsync(id);
            if (priceList == null)
            {
                return NotFound(new { Message = "Price list not found." });
            }

            _context.PriceLists.Remove(priceList);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Price list deleted successfully." });
        }
    }
}
