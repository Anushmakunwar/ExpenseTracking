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
    public class TransactionTagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionTagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/transactiontags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionTag>>> GetTransactionTags()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var transactionTags = await _context.TransactionTags
                .Include(tt => tt.Transaction)
                .Include(tt => tt.Tag)
                .Where(tt => tt.Transaction.UserId == userId)
                .ToListAsync();

            return Ok(transactionTags);
        }

        // GET: api/transactiontags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionTag>> GetTransactionTag(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var transactionTag = await _context.TransactionTags
                .Include(tt => tt.Transaction)
                .Include(tt => tt.Tag)
                .Where(tt => tt.Transaction.UserId == userId && tt.TransactionId == id)
                .FirstOrDefaultAsync();

            if (transactionTag == null)
            {
                return NotFound();
            }

            return Ok(transactionTag);
        }

        // POST: api/transactiontags
        [HttpPost]
        public async Task<ActionResult<TransactionTag>> CreateTransactionTag([FromBody] TransactionTag transactionTag)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var transaction = await _context.Transactions
                .Where(t => t.UserId == userId && t.Id == transactionTag.TransactionId)
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return Unauthorized();
            }

            _context.TransactionTags.Add(transactionTag);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTransactionTag", new { id = transactionTag.TransactionId }, transactionTag);
        }

        // DELETE: api/transactiontags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransactionTag(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var transactionTag = await _context.TransactionTags
                .Where(tt => tt.Transaction.UserId == userId && tt.TransactionId == id)
                .FirstOrDefaultAsync();

            if (transactionTag == null)
            {
                return NotFound();
            }

            _context.TransactionTags.Remove(transactionTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
