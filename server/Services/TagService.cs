using ExpenseTracker.Data;
using ExpenseTracker.Models;
using System.Collections.Generic;

namespace ExpenseTracker.Services
{
    public class TagService
    {
        private readonly ApplicationDbContext _context;

        public TagService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddTag(Tag tag)
        {
            _context.Tags.Add(tag);
            _context.SaveChanges();
        }

        public List<Tag> GetAllTags()
        {
            return _context.Tags.ToList();
        }
    }
}
