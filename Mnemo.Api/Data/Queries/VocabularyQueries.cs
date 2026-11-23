using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Shared.Enums;

namespace Mnemo.Data.Queries
{
    public class VocabularyQueries
    {
        private AppDbContext _context;


        public VocabularyQueries(AppDbContext context)
        {
            _context = context;
        }


        // Queries
        public IQueryable<Vocabulary> GetVocabByOwnerIdQuery(int ownerId)
            => _context.Vocabularies.Where(p => p.OwnerId == ownerId);

        public IQueryable<Vocabulary> GetVocabByIdQuery(int ownerId, int vocabId)
            => _context.Vocabularies.Where(p => p.OwnerId == ownerId && p.Id == vocabId);

        public IQueryable<Vocabulary> GetVocabByGuidQuery(int ownerId, Guid guid)
            => _context.Vocabularies.Where(p => p.OwnerId == ownerId && p.Guid == guid);

        public IQueryable<Vocabulary> GetVocabByEntryIdQuery(int ownerId, int entryId)
            => _context.VocabularyEntryLinks.Where(l => l.Vocabulary.OwnerId == ownerId && l.VocabularyEntryId == entryId).Select(l => l.Vocabulary);


        // Getters
        public async Task<bool> ExistsByGuidAsync(int ownerId, Guid guid)
            => await GetVocabByGuidQuery(ownerId, guid).AnyAsync();

        public async Task<Vocabulary?> GetByGuidAsync(int ownerId, Guid guid)
            => await GetVocabByGuidQuery(ownerId, guid).FirstOrDefaultAsync();

        public async Task<int?> GetIdByGuidAsync(int ownerId, Guid guid)
            => await GetVocabByGuidQuery(ownerId, guid).Select(v => v.Id).FirstOrDefaultAsync();

        public async Task<bool> ExistsByIdAsync(int ownerId, int vocabId)
            => await GetVocabByIdQuery(ownerId, vocabId).AnyAsync();

        public async Task<Vocabulary?> GetByIdAsync(int ownerId, int vocabId)
            => await GetVocabByIdQuery(ownerId, vocabId).FirstOrDefaultAsync();

        public async Task<List<Vocabulary>> GetByEntryIdAsync(int ownerId, int entryId)
            => await GetVocabByEntryIdQuery(ownerId, entryId).ToListAsync();
    }
}
