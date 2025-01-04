using ExpenseTracker.Models;
using ExpenseTracker.DTOs;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public interface IAuthService
    {
        Task<string> Register(RegisterModel model);
        Task<string> Login(LoginModel model);
    }
}
