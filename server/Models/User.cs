namespace ExpenseTracker.Models

{
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Currency { get; set; }
    public string Email { get; set; }
    public decimal Budget { get; set; } 
}
}