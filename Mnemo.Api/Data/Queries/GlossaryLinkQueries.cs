using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Shared.Enums;

namespace Mnemo.Data.Queries
{
    public class GlossaryLinkQueries
    {
        private AppDbContext _context;


        public GlossaryLinkQueries(AppDbContext context)
        {
            _context = context;
        }


        // Queries
        public IQueryable<GlossaryLink> GetByPairIdQuery(int glossId, int entryId)
            => _context.GlossaryLinks.Where(l => l.GlossaryId == glossId && l.EntryId == entryId);

        public IQueryable<GlossaryLink> GetByEntryIdQuery(int entryId)
            => _context.GlossaryLinks.Where(l => l.EntryId == entryId);

        // Getters
        public async Task<bool> ExistByEntryIdAsync(int entryId)
            => await GetByEntryIdQuery(entryId).AnyAsync();

        public async Task<bool> ExistByGlossaryIdAsync(int glossId)
            => await _context.GlossaryLinks.AnyAsync(e => e.GlossaryId == glossId);

        public async Task<GlossaryLink?> GetByPairIdAsync(int glossId, int entryId)
            => await GetByPairIdQuery(glossId, entryId).FirstOrDefaultAsync();

        public async Task<List<GlossaryLink>> GetLinksByEntryIdAsync(int entryId)
            => await GetByEntryIdQuery(entryId).ToListAsync();
    }
}
