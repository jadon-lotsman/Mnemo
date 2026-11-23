using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Shared.Enums;

namespace Mnemo.Data.Queries
{
    public class VocabularyEntryQueries
    {
        private AppDbContext _context;


        public VocabularyEntryQueries(AppDbContext context)
        {
            _context = context;
        }


        // Queries
        public IQueryable<VocabularyEntry> GetByOwnerIdQuery(int ownerId)
            => _context.VocabularyEntries.Where(e => e.OwnerId == ownerId);

        public IQueryable<VocabularyEntry> GetByVocabularyIdQuery(int ownerId, int vocabId)
            => _context.VocabularyEntryLinks.Where(l => l.Vocabulary.OwnerId == ownerId && l.VocabularyId == vocabId).Select(l => l.VocabularyEntry);

        public IQueryable<VocabularyEntry> GetByVocabularyGuidQuery(int ownerId, Guid guid)
            => _context.VocabularyEntryLinks.Where(l => l.Vocabulary.Guid == guid && l.Vocabulary.OwnerId == ownerId).Select(l => l.VocabularyEntry);

        // Getters
        public async Task<bool> HasAltPartOfSpeechAsync(int ownerId, string foreign, PartOfSpeech? partOfSpeech)
            => await GetByOwnerIdQuery(ownerId).AnyAsync(e => e.Foreign == foreign && e.PartOfSpeech != partOfSpeech);

        public async Task<bool> ExistsByIdAsync(int ownerId, int entryId)
            => await GetByOwnerIdQuery(ownerId).AnyAsync(e => e.Id == entryId);

        public async Task<bool> ExistsByKeyAsync(int ownerId, string foreign, PartOfSpeech? partOfSpeech)
            => await GetByOwnerIdQuery(ownerId).AnyAsync(e => e.Foreign == foreign && e.PartOfSpeech == partOfSpeech);

        public async Task<VocabularyEntry?> GetByIdAsync(int ownerId, int entryId)
            => await GetByOwnerIdQuery(ownerId).FirstOrDefaultAsync(e => e.Id == entryId);

        public async Task<VocabularyEntry?> GetInboxedByIdAsync(int ownerId, int entryId)
            => await GetByOwnerIdQuery(ownerId).FirstOrDefaultAsync(e => e.Id == entryId && !e.VocabularyLinks.Any());

        public async Task<Dictionary<(string foreign, PartOfSpeech? partOfSpeech), VocabularyEntry>> GetKeysByForeignsAsync(int ownerId, List<string> foreigns)
            => await GetByOwnerIdQuery(ownerId)
            .Where(e => foreigns.Contains(e.Foreign))
            .ToDictionaryAsync(k => (k.Foreign, k.PartOfSpeech), v => v);

        public async Task<HashSet<int>> GetAlreadyLinkedIdsAsync(int vocabId, List<int> entryIds)
        {
            if (entryIds.Count == 0)
                return new HashSet<int>();

            var ids = await _context.VocabularyEntryLinks
                .Where(l => l.VocabularyId == vocabId
                         && entryIds.Contains(l.VocabularyEntryId))
                .Select(l => l.VocabularyEntryId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task<List<VocabularyEntry>> GetByQueryAsync(int userId, string query, int limit = 20)
        {
            query = query.ToLower();

            return await GetByOwnerIdQuery(userId)
                .Where(e => e.Foreign.Contains(query) || e.Translations.Any(t => t.Contains(query)))
                .OrderBy(e => e.Id)
                .Take(limit)
                .ToListAsync();
        }
    }
}
