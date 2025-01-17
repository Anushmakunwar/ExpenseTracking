namespace ExpenseTracker.Models
{
    public class Transaction
    {
        public enum TransactionType
        {
            Credit,
            Debit,
            Debt
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }

        public TransactionType Type { get; set; }  // "Credit", "Debit", "Debt"

        public string? Notes { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        // Relationships
        public ICollection<TransactionTag>? TransactionTags { get; set; }
    }
}
