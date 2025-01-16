using ExpenseTracker.Data;
using ExpenseTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Services
{
    public class TransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddTransaction(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            _context.SaveChanges();
        }

        public List<Transaction> GetTransactionsByUserId(int userId, DateTime startDate, DateTime endDate)
        {
            return _context.Transactions
                           .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                           .ToList();
        }

        public decimal GetTotalAmount(int userId)
        {
            return _context.Transactions.Where(t => t.UserId == userId).Sum(t => t.Amount);
        }
    }
}
