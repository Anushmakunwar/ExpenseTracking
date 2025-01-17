using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TransactionTag> TransactionTags { get; set; }
    public DbSet<Debt> Debts { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the relationship between Transaction and Tag through the TransactionTag model.
        modelBuilder.Entity<TransactionTag>()
            .HasKey(tt => new { tt.TransactionId, tt.TagId });

        modelBuilder.Entity<TransactionTag>()
            .HasOne(tt => tt.Transaction)
            .WithMany(t => t.TransactionTags)
            .HasForeignKey(tt => tt.TransactionId);

        modelBuilder.Entity<TransactionTag>()
            .HasOne(tt => tt.Tag)
            .WithMany(t => t.TransactionTags)
            .HasForeignKey(tt => tt.TagId);

        // If you have other relationships, configure them here without calling DbContext or querying the database.
        modelBuilder.Entity<Tag>()
            .HasOne(t => t.User)
            .WithMany() 
            .HasForeignKey(t => t.UserId);
    }
}