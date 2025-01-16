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
        
        // Corrected the data type for Type property to TransactionType
        public TransactionType Type { get; set; }  // "Credit", "Debit", "Debt"
        
        public string? Notes { get; set; }   // Optional notes for the transaction
        public int UserId { get; set; }  // Foreign key to User
        public User? User { get; set; }   // Navigation property

        // Relationships
        public ICollection<TransactionTag>? TransactionTags { get; set; }  // One transaction can have many tags
    }
}
