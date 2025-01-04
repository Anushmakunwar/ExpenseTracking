using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DebtsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DebtsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/debts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Debt>>> GetDebts()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var debts = await _context.Debts
                .Where(d => d.UserId == userId)
                .ToListAsync();

            return Ok(debts);
        }

        // GET: api/debts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Debt>> GetDebt(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var debt = await _context.Debts
                .Where(d => d.UserId == userId && d.Id == id)
                .FirstOrDefaultAsync();

            if (debt == null)
            {
                return NotFound();
            }

            return Ok(debt);
        }

        // POST: api/debts
        [HttpPost]
        public async Task<ActionResult<Debt>> CreateDebt([FromBody] Debt debt)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            debt.UserId = userId;

            _context.Debts.Add(debt);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDebt", new { id = debt.Id }, debt);
        }

        // PUT: api/debts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDebt(int id, [FromBody] Debt debt)
        {
            if (id != debt.Id)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (debt.UserId != userId)
            {
                return Unauthorized();
            }

            _context.Entry(debt).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DebtExists(id))
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

        // DELETE: api/debts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDebt(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var debt = await _context.Debts
                .Where(d => d.UserId == userId && d.Id == id)
                .FirstOrDefaultAsync();

            if (debt == null)
            {
                return NotFound();
            }

            _context.Debts.Remove(debt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DebtExists(int id)
        {
            return _context.Debts.Any(d => d.Id == id);
        }
    }
}
