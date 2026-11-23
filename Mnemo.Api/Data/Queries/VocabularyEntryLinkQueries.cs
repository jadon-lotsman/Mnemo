using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Shared.Enums;

namespace Mnemo.Data.Queries
{
    public class VocabularyEntryLinkQueries
    {
        private AppDbContext _context;


        public VocabularyEntryLinkQueries(AppDbContext context)
        {
            _context = context;
        }


        // Queries
        public IQueryable<VocabularyEntryLink> GetLinksByIdsQuery(int vocabId, int entryId)
            => _context.VocabularyEntryLinks.Where(l => l.VocabularyId == vocabId && l.VocabularyEntryId == entryId);

        // Getters
        public async Task<bool> ExistByEntryIdAsync(int entryId)
            => await _context.VocabularyEntryLinks.AnyAsync(e => e.VocabularyEntryId == entryId);

        public async Task<bool> ExistByVocabularyIdAsync(int vocabId)
            => await _context.VocabularyEntryLinks.AnyAsync(e => e.VocabularyId == vocabId);

        public async Task<VocabularyEntryLink?> GetByPairIdAsync(int vocabId, int entryId)
            => await GetLinksByIdsQuery(vocabId, entryId).FirstOrDefaultAsync();
    }
}
