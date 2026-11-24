using Microsoft.EntityFrameworkCore;
using Mnemo.Entities;
using Mnemo.Shared.Enums;

namespace Mnemo.Infrastructure.Queries
{
    public class GlossaryLinkQueries
    {
        private AppDbContext _context;


        public GlossaryLinkQueries(AppDbContext context)
        {
            _context = context;
        }


        // Queries
        public IQueryable<GlossaryLink> GetByEntryIdQuery(Guid entryId)
            => _context.GlossaryLinks.Where(l => l.EntryId == entryId);

        public IQueryable<GlossaryLink> GetByGlossaryIdQuery(Guid glossId)
            => _context.GlossaryLinks.Where(l => l.GlossaryId == glossId);

        public IQueryable<GlossaryLink> GetByPairIdQuery(Guid glossId, Guid entryId)
            => _context.GlossaryLinks.Where(l => l.GlossaryId == glossId && l.EntryId == entryId);

        // Getters
        public async Task<bool> ExistByEntryIdAsync(Guid entryId)
            => await GetByEntryIdQuery(entryId).AnyAsync();

        public async Task<bool> ExistByGlossaryIdAsync(Guid glossId)
            => await GetByGlossaryIdQuery(glossId).AnyAsync();

        public async Task<bool> ExistByPairIdAsync(Guid glossId, Guid entryId)
            => await GetByPairIdQuery(glossId, entryId).AnyAsync();

        public async Task<List<GlossaryLink>> GetByEntryIdAsync(Guid entryId)
            => await GetByEntryIdQuery(entryId).ToListAsync();

        public async Task<List<GlossaryLink>> GetByGlossaryIdAsync(Guid glossId)
            => await GetByGlossaryIdQuery(glossId).ToListAsync();

        public async Task<GlossaryLink?> GetByPairIdAsync(Guid glossId, Guid entryId)
            => await GetByPairIdQuery(glossId, entryId).FirstOrDefaultAsync();
    }
}
