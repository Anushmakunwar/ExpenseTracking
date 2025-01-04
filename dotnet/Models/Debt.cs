namespace ExpenseTracker.Models
{
    public class Debt
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public decimal Amount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime DueDate { get; set; }
        public int UserId { get; set; }  // Foreign key to User
        public User User { get; set; }   // Navigation property
    }
}
