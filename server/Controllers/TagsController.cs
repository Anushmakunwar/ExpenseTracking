using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/tags
        [HttpGet]
        public async Task<ActionResult> GetTags([FromQuery] int page = 1, [FromQuery] int limit = 1000)
        {
            // Access the user information from the HttpContext.User
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID is missing or invalid.");
            }

            // Query to get tags belonging to the authenticated user
            var query = _context.Tags
                .Where(t => t.UserId == userId)
                .AsQueryable();

            // Get the total count of tags before applying pagination
            int totalCount = await query.CountAsync();

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalCount / limit);

            // Apply pagination
            var tags = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // Return paginated response
            return Ok(new
            {
                Page = page,
                Limit = limit,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Data = tags
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTag([FromBody] Tag tag)
        {
            if (tag == null)
            {
                return BadRequest("Tag data is null");
            }

            // Retrieve the user based on the userId from the claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine(userIdString);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID is missing or invalid.");
            }

            // Assign the userId from claims to the tag model
            tag.UserId = userId;

            // Add the tag to the database
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTags), new { id = tag.Id }, tag);
        }



        // PUT: api/tags/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] Tag tag)
        {
            if (id != tag.Id)
            {
                return BadRequest();
            }

            // Access the user information from the HttpContext.User
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID is missing or invalid.");
            }

            if (tag.UserId != userId)
            {
                return Unauthorized();
            }

            _context.Entry(tag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/tags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            // Access the user information from the HttpContext.User
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID is missing or invalid.");
            }

            var tag = await _context.Tags
                .Where(t => t.UserId == userId && t.Id == id)
                .FirstOrDefaultAsync();

            if (tag == null)
            {
                return NotFound();
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TagExists(int id)
        {
            return _context.Tags.Any(t => t.Id == id);
        }
    }
}
