using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class Debt
    {
        [Key]
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public string Source { get; set; }

        public bool IsCleared { get; set; } = false;

        public int UserId { get; set; }

        public User User { get; set; }
    }
}
