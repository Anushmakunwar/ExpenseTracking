namespace ExpenseTracker.Models
{
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int UserId { get; set; }  // Foreign key to User
    public User? User { get; set; }   // Navigation property
    // Relationships
    public ICollection<TransactionTag>? TransactionTags { get; set; }  // Many-to-many relationship with Transaction
}
}