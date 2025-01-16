using ExpenseTracker.Data;
using ExpenseTracker.Models;
using System;
using System.Linq;

namespace ExpenseTracker.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public decimal GetTotalInflow(int userId)
        {
            return _context.Transactions
                .Where(t => t.UserId == userId && t.Type == Transaction.TransactionType.Credit)  // Reference directly
                .Sum(t => t.Amount);
        }

        public decimal GetTotalOutflow(int userId)
        {
            return _context.Transactions
                .Where(t => t.UserId == userId && t.Type == Transaction.TransactionType.Debit)  // Reference directly
                .Sum(t => t.Amount);
        }

        public decimal GetTotalDebt(int userId)
        {
            return _context.Debts
                .Where(d => d.UserId == userId && !d.IsCleared)
                .Sum(d => d.Amount);
        }
    }
}
