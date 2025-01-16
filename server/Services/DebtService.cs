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
        public List<Debt> GetPendingDebts(int userId)
        {
            return _context.Debts
                .Where(d => d.UserId == userId && !d.IsCleared)
                .ToList();
        }
    }
}
