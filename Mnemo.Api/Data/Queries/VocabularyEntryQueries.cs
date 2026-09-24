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
        public IQueryable<VocabularyEntry> GetEntriesByOwnerIdQuery(int ownerId)
            => _context.VocabularyEntries.Where(e => e.OwnerId == ownerId);

        public IQueryable<VocabularyEntry> GetEntriesOfActiveVocabulariesByOwnerIdQuery(int ownerId)
            => _context.VocabularyEntryLinks.Where(l => l.Vocabulary.OwnerId == ownerId && l.Vocabulary.IsActive).SelectMany(l => _context.VocabularyEntries.Where(e => e.Id == l.VocabularyEntryId));

        public IQueryable<VocabularyEntry> GetEntriesByVocabularyIdQuery(int ownerId, int vocabId)
            => _context.VocabularyEntryLinks.Where(l => l.Vocabulary.OwnerId == ownerId && l.VocabularyId == vocabId).Select(l => l.VocabularyEntry);

        public IQueryable<VocabularyEntry> GetEntriesByVocabularyGuidQuery(int ownerId, Guid guid)
            => _context.VocabularyEntryLinks.Where(l => l.Vocabulary.Guid == guid && (l.Vocabulary.Visibility != Visibility.Private || l.Vocabulary.OwnerId == ownerId)).Select(l => l.VocabularyEntry);


        // Getters
        public async Task<bool> HasAlternativePartOfSpeechAsync(int ownerId, string foreign, PartOfSpeech? partOfSpeech)
            => await GetEntriesByOwnerIdQuery(ownerId).AnyAsync(e => e.Foreign == foreign && e.PartOfSpeech != partOfSpeech);

        public async Task<bool> ExistsByKeyAsync(int ownerId, string foreign, PartOfSpeech? partOfSpeech)
            => await GetEntriesByOwnerIdQuery(ownerId).AnyAsync(e => e.Foreign == foreign && e.PartOfSpeech == partOfSpeech);

        public async Task<VocabularyEntry?> GetByIdAsync(int ownerId, int id)
            => await GetEntriesByOwnerIdQuery(ownerId).FirstOrDefaultAsync(e => e.Id == id);

        public async Task<Dictionary<(string foreign, PartOfSpeech? partOfSpeech), VocabularyEntry>> GetKeysByForeignsAsync(int ownerId, List<string> foreigns)
            => await GetEntriesByOwnerIdQuery(ownerId)
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

            return await GetEntriesByOwnerIdQuery(userId)
                .Where(e => e.Foreign.Contains(query) || e.Translations.Any(t => t.Contains(query)))
                .OrderBy(e => e.Id)
                .Take(limit)
                .ToListAsync();
        }
    }
}
