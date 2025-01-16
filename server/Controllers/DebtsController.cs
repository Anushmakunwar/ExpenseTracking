using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Services;
using ExpenseTracker.Models;
using System.Security.Claims;

namespace ExpenseTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebtController : ControllerBase
    {
        private readonly DebtService _debtService;

        public DebtController(DebtService debtService)
        {
            _debtService = debtService;
        }

        // Endpoint to create a debt
        [HttpPost]
        public IActionResult CreateDebt([FromBody] Debt debt)
        {
            _debtService.AddDebt(debt);
            return Ok(debt);
        }

        // Endpoint to clear a debt
        [HttpPost("ClearDebt")]
        public IActionResult ClearDebt(int debtId)
        {
            _debtService.ClearDebt(debtId);
            return Ok();
        }

        // Endpoint to list pending debts
        [HttpGet("PendingDebts")]
        public IActionResult GetPendingDebts()
        {
            try
            {
                // Extracting userId from claims (make sure it's stored as NameIdentifier or modify accordingly)
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // If userId is null or empty, return Unauthorized
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated.");
                }

                // Convert to integer if valid
                if (!int.TryParse(userId, out int parsedUserId))
                {
                    return BadRequest("Invalid user ID.");
                }

                var pendingDebts = _debtService.GetPendingDebts(parsedUserId);

                if (pendingDebts == null || pendingDebts.Count == 0)
                {
                    return NotFound("No pending debts found.");
                }

                return Ok(pendingDebts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

         [HttpGet("AllDebts")]
        public IActionResult GetAllDebts()
        {
            try
            {
                // Extracting userId from claims (ensure it's stored as NameIdentifier or modify accordingly)
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // If userId is null or empty, return Unauthorized
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated.");
                }

                // Convert to integer if valid
                if (!int.TryParse(userId, out int parsedUserId))
                {
                    return BadRequest("Invalid user ID.");
                }

                // Get the list of all debts (pending or fulfilled) for the authenticated user
                var allDebts = _debtService.GetAllDebts(parsedUserId);

                // Check if any debts exist
                if (allDebts == null || allDebts.Count == 0)
                {
                    return NotFound("No debts found.");
                }

                // Return the list of all debts
                return Ok(allDebts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        
    }
    }
}
