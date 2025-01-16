namespace ExpenseTracker.DTOs
{
    public class UserDetailsDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }
        public decimal Budget { get; set; }
    }
}
