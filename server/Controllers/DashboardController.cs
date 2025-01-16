using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Services;

namespace ExpenseTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public IActionResult GetDashboardStats(int userId)
        {
            var inflow = _dashboardService.GetTotalInflow(userId);
            var outflow = _dashboardService.GetTotalOutflow(userId);
            var totalDebt = _dashboardService.GetTotalDebt(userId);
            return Ok(new { Inflow = inflow, Outflow = outflow, TotalDebt = totalDebt });
        }
    }
}
