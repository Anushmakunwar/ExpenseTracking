using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: api/transactions/filter
        [HttpGet("filter")]
        public async Task<ActionResult> FilterTransactions(
            [FromQuery] string? type,
            [FromQuery] List<string>? tagNames,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? title,
            [FromQuery] string? sortBy,
            [FromQuery] bool? isAscending = false,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10
        )
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var query = _context.Transactions
                    .Include(t => t.TransactionTags)
                        .ThenInclude(tt => tt.Tag)
                    .Where(t => t.UserId == userId)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(type) && Enum.TryParse(type, true, out Transaction.TransactionType transactionType))
                {
                    query = query.Where(t => t.Type == transactionType);
                }

                if (tagNames != null && tagNames.Any())
                {
                    query = query.Where(t => t.TransactionTags.Any(tt => tagNames.Contains(tt.Tag.Name)));
                }

                if (startDate.HasValue)
                {
                    query = query.Where(t => t.Date >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(t => t.Date <= endDate.Value);
                }

                int totalCount = await query.CountAsync();

                int totalPages = (int)Math.Ceiling((double)totalCount / limit);

                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "title":
                            query = isAscending.HasValue && isAscending.Value
                                ? query.OrderBy(t => t.Title)
                                : query.OrderByDescending(t => t.Title);
                            break;
                        case "amount":
                            query = isAscending.HasValue && isAscending.Value
                                ? query.OrderBy(t => t.Amount)
                                : query.OrderByDescending(t => t.Amount);
                            break;
                        case "date":
                            query = isAscending.HasValue && isAscending.Value
                                ? query.OrderBy(t => t.Date)
                                : query.OrderByDescending(t => t.Date);
                            break;
                        default:
                            query = query.OrderByDescending(t => t.Date);
                            break;
                    }
                }
                else
                {
                    query = query.OrderByDescending(t => t.Date);
                }

                // Apply pagination
                var transactions = await query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var transactionDtos = transactions.Select(t => new TransactionResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Amount = t.Amount,
                    Date = t.Date,
                    Type = t.Type.ToString(),
                    Notes = t.Notes,
                    Tags = t.TransactionTags.Select(tt => tt.Tag.Name).ToList()
                }).ToList();

                // Return paginated response
                return Ok(new
                {
                    Page = page,
                    Limit = limit,
                    TotalPages = totalPages,
                    TotalCount = totalCount,
                    Data = transactionDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        // GET: api/transactions/transaction-summary
        [HttpGet("transaction-summary")]
        public async Task<ActionResult<TransactionSummaryResponseDto>> GetTransactionSummaryResponse()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Get all transactions for the user
                var transactions = await _context.Transactions
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

                // Get highest and lowest inflow (Credit and Debt)
                var inflows = transactions
                    .Where(t => t.Type == Transaction.TransactionType.Credit || t.Type == Transaction.TransactionType.Debt)
                    .ToList();
                var highestInflow = inflows.OrderByDescending(t => t.Amount).FirstOrDefault();
                var lowestInflow = inflows.OrderBy(t => t.Amount).FirstOrDefault();

                // Get highest and lowest outflow (Debit)
                var outflows = transactions
                    .Where(t => t.Type == Transaction.TransactionType.Debit)
                    .ToList();
                var highestOutflow = outflows.OrderByDescending(t => t.Amount).FirstOrDefault();
                var lowestOutflow = outflows.OrderBy(t => t.Amount).FirstOrDefault();

                // Get highest and lowest debt (Debt)
                var debts = transactions
                    .Where(t => t.Type == Transaction.TransactionType.Debt)
                    .ToList();
                var highestDebt = debts.OrderByDescending(t => t.Amount).FirstOrDefault();
                var lowestDebt = debts.OrderBy(t => t.Amount).FirstOrDefault();

                var summary = new TransactionSummaryResponseDto
                {
                    HighestInflow = highestInflow == null ? null : new TransactionDto(highestInflow),
                    LowestInflow = lowestInflow == null ? null : new TransactionDto(lowestInflow),
                    HighestOutflow = highestOutflow == null ? null : new TransactionDto(highestOutflow),
                    LowestOutflow = lowestOutflow == null ? null : new TransactionDto(lowestOutflow),
                    HighestDebt = highestDebt == null ? null : new TransactionDto(highestDebt),
                    LowestDebt = lowestDebt == null ? null : new TransactionDto(lowestDebt)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        // GET: api/transactions/summary
        [HttpGet("summary")]
        public async Task<ActionResult<TransactionSummaryDto>> GetTransactionSummaryDto()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Get all transactions for the user
                var transactions = await _context.Transactions
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

                // Calculate total number of transactions
                var totalTransactions = transactions.Count;

                // Calculate total transaction value (inflows + debts - outflows)
                var totalValue = transactions
                    .Where(t => t.Type == Transaction.TransactionType.Credit || t.Type == Transaction.TransactionType.Debt)
                    .Sum(t => t.Amount)  // Add inflows and debts
                    - transactions
                    .Where(t => t.Type == Transaction.TransactionType.Debit)
                    .Sum(t => t.Amount); // Subtract outflows

                var summary = new TransactionSummaryDto
                {
                    TotalTransactions = totalTransactions,
                    TotalValue = totalValue
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        // GET: api/transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return Ok(transactions);
        }

        // GET: api/transactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var transaction = await _context.Transactions
                .Where(t => t.UserId == userId && t.Id == id)
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound();
            }

            return Ok(transaction);
        }

        // POST: api/transactions
        [HttpPost]
        public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request body.");
            }

            if (string.IsNullOrEmpty(request.Title) || request.Amount <= 0)
            {
                return BadRequest("Title and Amount must be provided, and Amount must be greater than zero.");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if the user has an existing budget (e.g., a "Budget" table or column in the user table)
            var userBudget = await _context.Users.Where(u => u.Id == userId).Select(u => u.Budget).FirstOrDefaultAsync();

            if (userBudget == null)
            {
                return BadRequest("User budget not found.");
            }

            // Create the transaction object
            var transaction = new Transaction
            {
                Title = request.Title,
                Amount = request.Amount,
                Date = DateTime.UtcNow,
                Type = Enum.TryParse(request.Type, true, out Transaction.TransactionType type) ? type : Transaction.TransactionType.Debit,
                Notes = request.Notes,
                UserId = userId
            };

            // Handle Debit transaction
            if (transaction.Type == Transaction.TransactionType.Debit)
            {
                // Check if the user has enough budget
                if (userBudget < transaction.Amount)
                {
                    return BadRequest("Insufficient funds to complete this transaction.");
                }

                // Update the user's budget after a debit transaction
                userBudget -= transaction.Amount;
                var user = await _context.Users.FindAsync(userId);
                user.Budget = userBudget;
                _context.Users.Update(user);
            }

                // Handle Credit transaction and clear matching debt
    if (transaction.Type == Transaction.TransactionType.Credit)
    {
        var matchingDebt = await _context.Debts
            .Where(d => d.UserId == userId && !d.IsCleared && d.Amount == transaction.Amount)
            .OrderBy(d => d.DueDate) // Optional: prioritize based on due date
            .FirstOrDefaultAsync();

        if (matchingDebt != null)
        {
            matchingDebt.IsCleared = true;
            _context.Debts.Update(matchingDebt);
        }

        userBudget += transaction.Amount;
        var user = await _context.Users.FindAsync(userId);
        user.Budget = userBudget;
        _context.Users.Update(user);
    }
         
            // If the transaction type is Debt, create a corresponding Debt entry
            if (transaction.Type == Transaction.TransactionType.Debt)
            {
                var debt = new Debt
                {
                    Amount = request.Amount,
                    DueDate = request.DueDate ?? DateTime.MinValue,   
                    Source = request.Source,   
                    UserId = userId,
                    IsCleared = false  
                };

                _context.Debts.Add(debt);  
            }

            // Handle Tags (optional, based on your existing logic)
            if (request.TagIds != null && request.TagIds.Any())
            {
                var validTags = await _context.Tags.Where(tag => request.TagIds.Contains(tag.Id)).ToListAsync();

                if (validTags.Count != request.TagIds.Count)
                {
                    return BadRequest("Some provided TagIds are invalid.");
                }

                transaction.TransactionTags = validTags.Select(tag => new TransactionTag
                {
                    TagId = tag.Id
                }).ToList();
            }

            try
            {
                // Add transaction to the database
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                // Reload the transaction to include related data
                var createdTransaction = await _context.Transactions
                    .Include(t => t.TransactionTags)
                    .ThenInclude(tt => tt.Tag)
                    .FirstOrDefaultAsync(t => t.Id == transaction.Id);

                return CreatedAtAction("GetTransaction", new { id = createdTransaction.Id }, createdTransaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // GET: api/transactions/details
        [HttpGet("details")]
        public async Task<ActionResult<TransactionDetailsDto>> GetTransactionDetails()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Get all transactions for the user
                var transactions = await _context.Transactions
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

                // Calculate total inflows (Credit + Debt transactions)
                var totalInflows = transactions
                    .Where(t => t.Type == Transaction.TransactionType.Credit || t.Type == Transaction.TransactionType.Debt)
                    .Sum(t => t.Amount);

                // Calculate total outflows (Debit transactions)
                var totalOutflows = transactions
                    .Where(t => t.Type == Transaction.TransactionType.Debit)
                    .Sum(t => t.Amount);

                // Calculate total debt (Debt transactions)
                var totalDebt = transactions
                    .Where(t => t.Type == Transaction.TransactionType.Debt)
                    .Sum(t => t.Amount);

                // Fetch all debts for the user and calculate cleared and remaining debt
                var debts = await _context.Debts
                    .Where(d => d.UserId == userId)
                    .ToListAsync();

                // Calculate cleared debt (only debts marked as cleared)
                var clearedDebt = debts
                    .Where(d => d.IsCleared) // Check for true explicitly if IsCleared is nullable
                    .Sum(d => d.Amount);

                // Calculate remaining debt (only debts that are not cleared)
                var remainingDebt = debts
                    .Where(d => !d.IsCleared)
                    .Sum(d => d.Amount);

                var details = new TransactionDetailsDto
                {
                    TotalInflows = totalInflows,
                    TotalOutflows = totalOutflows,
                    TotalDebt = totalDebt,
                    ClearedDebt = clearedDebt,
                    RemainingDebt = remainingDebt
                };

                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }





        // PUT: api/transactions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (transaction.UserId != userId)
            {
                return Unauthorized();
            }

            _context.Entry(transaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/transactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var transaction = await _context.Transactions
                .Where(t => t.UserId == userId && t.Id == id)
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(t => t.Id == id);
        }
    }

    public class CreateTransactionRequest
    {
        public string Title { get; set; }
        public decimal Amount { get; set; }
        // public DateTime? Date { get; set; }
        public string Type { get; set; }
        public string? Notes { get; set; }

        public DateTime? DueDate { get; set; } // Add this line
        public string? Source { get; set; }     // Add this line
        public List<int>? TagIds { get; set; }
    }
    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Notes { get; set; }
        public List<string> Tags { get; set; }  // List of tag names
    }

    public class TransactionSummaryDto
    {
        public int TotalTransactions { get; set; }
        public decimal TotalValue { get; set; }
    }


    public class TransactionDetailsDto
    {
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal ClearedDebt { get; set; }
        public decimal RemainingDebt { get; set; }
    }

    public class TransactionSummaryResponseDto
    {
        public TransactionDto? HighestInflow { get; set; }
        public TransactionDto? LowestInflow { get; set; }
        public TransactionDto? HighestOutflow { get; set; }
        public TransactionDto? LowestOutflow { get; set; }
        public TransactionDto? HighestDebt { get; set; }
        public TransactionDto? LowestDebt { get; set; }
    }

    public class TransactionResponseDtoList
    {
        public List<TransactionResponseDto> Data { get; set; } = new List<TransactionResponseDto>();
    }

    public class TransactionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string? Notes { get; set; }

        public TransactionDto(Transaction transaction)
        {
            Id = transaction.Id;
            Title = transaction.Title;
            Amount = transaction.Amount;
            Date = transaction.Date;
            Type = transaction.Type.ToString();
            Notes = transaction.Notes;
        }


    }




}