using ExpenseTracker.DTOs;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public AuthController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context; // Proper initialization
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var token = await _authService.Register(model);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/users
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Unauthorized(); // User not found
            }

            if (!string.IsNullOrEmpty(model.Username))
                user.Username = model.Username;

            if (!string.IsNullOrEmpty(model.Email))
                user.Email = model.Email;

            if (model.Budget.HasValue)
                user.Budget = model.Budget.Value;

            try
            {
                await _context.SaveChangesAsync(); // Save changes to the database
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user.", details = ex.Message });
            }

            return Ok(new { message = "User details updated successfully." });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var token = await _authService.Login(model);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/auth/userdetails
        [HttpGet("userdetails")]
        public async Task<ActionResult<UserDetailsDTO>> GetUserDetails()
        {
            // Get the user ID from the token
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Find the user in the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Unauthorized(); // User not found
            }

            // Return user details in the DTO format
            var userDetails = new UserDetailsDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Currency = user.Currency,
                Budget = user.Budget
            };

            return Ok(userDetails); // Return the user details to the frontend
        }
    }
}
