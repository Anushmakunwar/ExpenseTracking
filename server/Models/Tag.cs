namespace ExpenseTracker.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
        // Relationships
        public ICollection<TransactionTag>? TransactionTags { get; set; }  // Many-to-many relationship with Transaction
    }
}