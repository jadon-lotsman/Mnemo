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
        public IQueryable<VocabularyEntry> GetByUserIdQuery(int userId)
            => _context.Entries.Where(e => e.User.Id == userId);


        // Getters
        public async Task<bool> ExistsByIdAsync(int userId, int id)
            => await GetByUserIdQuery(userId).AnyAsync(e => e.Id == id);

        public async Task<bool> ExistsByKeysAsync(int userId, string foreign, PartOfSpeech? partOfSpeech)
            => await GetByUserIdQuery(userId).AnyAsync(e => e.Foreign == foreign && e.PartOfSpeech == partOfSpeech);

        public async Task<bool> HasAlternativePartOfSpeechAsync(int userId, string foreign, PartOfSpeech? partOfSpeech)
            => await GetByUserIdQuery(userId).AnyAsync(e => e.Foreign == foreign && e.PartOfSpeech != partOfSpeech);


        public async Task<VocabularyEntry?> GetByIdAsync(int userId, int id)
            => await GetByUserIdQuery(userId).FirstOrDefaultAsync(e => e.Id == id);

        public async Task<Dictionary<int, VocabularyEntry>> GetDictByIdsAsync(int userId, IEnumerable<int> ids)
        {
            var list = await GetByUserIdQuery(userId)
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();

            return list.ToDictionary(e => e.Id);
        }

        public async Task<List<VocabularyEntry>> GetByQueryAsync(int userId, string query, int limit = 20)
        {
            query = query.ToLower();

            return await GetByUserIdQuery(userId)
                .Where(e => e.Foreign.Contains(query) || e.Translations.Any(t => t.Contains(query)))
                .OrderBy(e => e.Id)
                .Take(limit)
                .ToListAsync();
        }
    }
}
