using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

// POST: api/expenses
    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] Expense expense)
    {
    // Get the user ID from the token
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    // Find the user in the database
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
    {
        return Unauthorized(); // User not found
    }

    // Check if the user's budget is sufficient
    if (user.Budget < expense.Amount)
    {
        return BadRequest(new { message = "Insufficient budget to create this expense." });
    }

    // Deduct the expense amount from the user's budget
    user.Budget -= expense.Amount;

    // Assign the user ID to the expense
    expense.UserId = userId;

    // Save the expense to the database
    _context.Expenses.Add(expense);

    try
    {
        await _context.SaveChangesAsync(); // Save both the expense and budget update
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "An error occurred while saving the expense.", details = ex.Message });
    }

    return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expense);
}


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return Ok(expenses);
        }

        // GET: api/expenses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var expense = await _context.Expenses
                .Where(e => e.UserId == userId && e.Id == id)
                .FirstOrDefaultAsync();

            if (expense == null)
            {
                return NotFound();
            }

            return Ok(expense);
        }
    }
}
