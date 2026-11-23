using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Shared.Enums;

namespace Mnemo.Data.Queries
{
    public class GlossaryEntryQueries
    {
        private AppDbContext _context;


        public GlossaryEntryQueries(AppDbContext context)
        {
            _context = context;
        }


        // Queries
        public IQueryable<GlossaryEntry> GetByOwnerIdQuery(int ownerId)
            => _context.GlossaryEntries.Where(e => e.OwnerId == ownerId);

        public IQueryable<GlossaryEntry> GetByGlossaryIdQuery(int ownerId, int glossId)
            => _context.GlossaryLinks.Where(l => l.Glossary.OwnerId == ownerId && l.GlossaryId == glossId).Select(l => l.Entry);

        public IQueryable<GlossaryEntry> GetByGlossaryGuidQuery(int ownerId, Guid guid)
            => _context.GlossaryLinks.Where(l => l.Glossary.Guid == guid && l.Glossary.OwnerId == ownerId).Select(l => l.Entry);

        // Getters
        public async Task<bool> HasAltPartOfSpeechAsync(int ownerId, string foreign, PartOfSpeech? partOfSpeech)
            => await GetByOwnerIdQuery(ownerId).AnyAsync(e => e.Foreign == foreign && e.PartOfSpeech != partOfSpeech);

        public async Task<bool> ExistsByIdAsync(int ownerId, int entryId)
            => await GetByOwnerIdQuery(ownerId).AnyAsync(e => e.Id == entryId);

        public async Task<bool> ExistsByKeyAsync(int ownerId, string foreign, PartOfSpeech? partOfSpeech)
            => await GetByOwnerIdQuery(ownerId).AnyAsync(e => e.Foreign == foreign && e.PartOfSpeech == partOfSpeech);

        public async Task<GlossaryEntry?> GetByIdAsync(int ownerId, int entryId)
            => await GetByOwnerIdQuery(ownerId).FirstOrDefaultAsync(e => e.Id == entryId);

        public async Task<GlossaryEntry?> GetInboxedByIdAsync(int ownerId, int entryId)
            => await GetByOwnerIdQuery(ownerId).FirstOrDefaultAsync(e => e.Id == entryId && !e.Links.Any());

        public async Task<Dictionary<(string foreign, PartOfSpeech? partOfSpeech), GlossaryEntry>> GetKeysByForeignsAsync(int ownerId, List<string> foreigns)
            => await GetByOwnerIdQuery(ownerId)
            .Where(e => foreigns.Contains(e.Foreign))
            .ToDictionaryAsync(k => (k.Foreign, k.PartOfSpeech), v => v);

        public async Task<HashSet<int>> GetAlreadyLinkedIdsAsync(int glossId, List<int> entryIds)
        {
            if (entryIds.Count == 0)
                return new HashSet<int>();

            var ids = await _context.GlossaryLinks
                .Where(l => l.GlossaryId == glossId
                         && entryIds.Contains(l.EntryId))
                .Select(l => l.EntryId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task<List<GlossaryEntry>> GetByQueryAsync(int userId, string query, int limit = 20)
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
