using ExpenseTracker.Models;
using ExpenseTracker.DTOs;
using ExpenseTracker.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher; // Use PasswordHasher<User>

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>(); // Initialize PasswordHasher
        }

        public async Task<string> Register(RegisterModel model)
        {
            // Validate input fields
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Email))
            {
                throw new ArgumentException("Username, password, and email are required.");
            }

            // Check if the user already exists
            var userExists = await _context.Users
                .AnyAsync(u => u.Username == model.Username || u.Email == model.Email);
            
            if (userExists)
            {
                throw new Exception("User with this username or email already exists.");
            }

            // Create a new user object
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Currency = model.Currency, // Assuming Currency is a required field
                Budget =0// Assuming Budget is a required field
            };

            // Hash the password before storing it
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password); // Pass the user object

            // Add the user to the database
            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync(); // Save the user to the database
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving the user to the database.", ex); // Catch and log any database issues
            }

            // Return the JWT token upon successful registration
            return GenerateJwtToken(user);
        }

        public async Task<string> Login(LoginModel model)
        {
            // Check if the user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);
            
            if (user == null)
                throw new Exception("Invalid credentials: User not found.");

            // Verify the password
            if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
                throw new Exception("Invalid credentials: Incorrect password.");

            // Generate and return the JWT token
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email) // Adding email as a claim
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
