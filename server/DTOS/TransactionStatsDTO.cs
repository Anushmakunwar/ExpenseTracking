using System;
using System.Collections.Generic;

namespace ExpenseTracker.Models
{
    public class TransactionStatsDTO
    {
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal TotalDebts { get; set; }
        public decimal ClearedDebts { get; set; }
        public decimal RemainingDebts { get; set; }
        public decimal HighestInflow { get; set; }
        public decimal HighestOutflow { get; set; }
        public decimal HighestDebt { get; set; }
        public List<DebtDTO> PendingDebts { get; set; }
    }

    public class DebtDTO
    {
        public string Source { get; set; }
        public DateTime DueDate { get; set; }
    }
}
