namespace ExpenseTracker.Models
{
    public class Expense
    {
        public int Id { get; set; }            // Primary key
        public string Name { get; set; }       // Name of the expense (e.g., Rent)
        public decimal Amount { get; set; }    // Amount of the expense
        public DateTime Date { get; set; }     // Date of the expense
         public int UserId { get; set; } 
    }
}
