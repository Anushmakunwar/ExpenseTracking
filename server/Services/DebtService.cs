using ExpenseTracker.Data;
using ExpenseTracker.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ExpenseTracker.Services
{
    public class DebtService
    {
        private readonly ApplicationDbContext _context;

        public DebtService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new debt
        public void AddDebt(Debt debt)
        {
            _context.Debts.Add(debt);
            _context.SaveChanges();
        }

        // Get total pending debt for a specific user
        public decimal GetTotalDebt(int userId)
        {
            return _context.Debts
                .Where(d => d.UserId == userId && !d.IsCleared)
                .Sum(d => d.Amount);
        }

        // Mark a debt as cleared
        public void ClearDebt(int debtId)
        {
            var debt = _context.Debts.Find(debtId);
            if (debt != null)
            {
                debt.IsCleared = true;
                _context.SaveChanges();
            }
        }

 public List<Debt> GetAllDebts(int userId)
        {
            return _context.Debts
                .Where(d => d.UserId == userId) // Filter by userId
                .ToList(); // No filter on IsCleared, returns all debts
        }
        // Get pending debts (debts that are not cleared)



     public (List<Debt> Debts, int TotalPages) GetPendingDebts(int userId, int page = 1, int limit = 5)
{
    // Calculate the total number of debts for pagination
    int totalDebts = _context.Debts
        .Count(d => d.UserId == userId && !d.IsCleared);

    // Calculate total pages (ensure no division by zero and round up)
    int totalPages = (int)Math.Ceiling((double)totalDebts / limit);

    // Calculate the number of items to skip based on the page and limit
    int skip = (page - 1) * limit;

    // Fetch the paginated debts
    var debts = _context.Debts
        .Where(d => d.UserId == userId && !d.IsCleared)
        .Skip(skip) // Skip items from previous pages
        .Take(limit) // Take only the items for the current page
        .ToList();

    return (debts, totalPages);
}

    }
}
