using Microsoft.EntityFrameworkCore;
using Mnemo.Data;
using Mnemo.Data.Entities;

namespace Mnemo.Services.Queries
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

        public IQueryable<VocabularyEntry> GetRandomByUserIdQuery(int userId, int? take = null, params int[] excludeIds)
        {
            var query = GetByUserIdQuery(userId);

            if (excludeIds != null && excludeIds.Any())
                query = query.Where(e => !excludeIds.Contains(e.Id));

            if (take.HasValue)
                query = query.Take(take.Value);

            return query
                .OrderBy(e => EF.Functions.Random());
        }


        // Getters
        public async Task<bool> ExistsByIdAsync(int userId, int id)
            => await GetByUserIdQuery(userId).AnyAsync(e => e.Id == id);

        public async Task<bool> ExistsByForeignAsync(int userId, string foreign)
            => await GetByUserIdQuery(userId).AnyAsync(e => e.Foreign == foreign);


        public async Task<VocabularyEntry?> GetByIdAsync(int userId, int id)
            => await GetByUserIdQuery(userId).FirstOrDefaultAsync(e => e.Id == id);

        public async Task<Dictionary<int, VocabularyEntry>> GetDictByIdsAsync(int userId, IEnumerable<int> ids)
        {
            var list = await GetByUserIdQuery(userId)
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();

            return list.ToDictionary(e => e.Id);
        }

        public async Task<VocabularyEntry?> GetByForeignAsync(int userId, string foreign)
            => await GetByUserIdQuery(userId).FirstOrDefaultAsync(e => string.Equals(e.Foreign, foreign));

        public async Task<List<VocabularyEntry>> GetLikeByForeignAndTranslationsAsync(int userId, string query)
            => await GetByUserIdQuery(userId)
            .Where(e => e.Foreign.Contains(query))
            .ToListAsync();

        public async Task<List<VocabularyEntry>> GetEntriesWithoutRepetitionStateAsync(int userId)
            => await _context.Entries.Where(e => e.User.Id == userId)
            .Where(e => e.RepetitionState == null)
            .ToListAsync();
    }
}
