using Microsoft.EntityFrameworkCore;
using Mnemo.Entities;
using Mnemo.Shared.Enums;

namespace Mnemo.Infrastructure.Queries
{
    public class GlossaryQueries
    {
        private AppDbContext _context;


        public GlossaryQueries(AppDbContext context)
        {
            _context = context;
        }


        // Queries
        public IQueryable<Glossary> GetByOwnerIdQuery(int ownerId)
            => _context.Glossaries.Where(g => g.OwnerId == ownerId);

        // Getters
        public async Task<bool> ExistsByIdAsync(int ownerId, Guid glossId)
            => await GetByOwnerIdQuery(ownerId).AnyAsync(g => g.Id == glossId);

        public async Task<Glossary?> GetByIdAsync(int ownerId, Guid glossId)
            => await GetByOwnerIdQuery(ownerId).FirstOrDefaultAsync(g => g.Id == glossId);

        public async Task<List<Glossary>> GetByEntryIdAsync(int ownerId, int entryId)
            => await _context.GlossaryLinks.Where(l => l.Glossary.OwnerId == ownerId && l.EntryId == entryId).Select(l => l.Glossary).ToListAsync();
    }
}
