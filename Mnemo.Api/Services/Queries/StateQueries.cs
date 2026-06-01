using Microsoft.EntityFrameworkCore;
using Mnemo.Data;
using Mnemo.Data.Entities;

namespace Mnemo.Services.Queries
{
    public class StateQueries
    {
        private AppDbContext _context;


        public StateQueries(AppDbContext context)
        {
            _context = context;
        }



        private IQueryable<RepetitionState> GetByUserIdQuery(int userId)
            => _context.RepetitionStates.Where(s => s.UserId == userId);


        public async Task<RepetitionState?> GetByEntryIdAsync(int userId, int entryId)
            => await GetByUserIdQuery(userId).FirstOrDefaultAsync(s => s.VocabularyEntryId == entryId);

        public async Task<List<RepetitionState>> GetAllByUserIdAsync(int userId)
            => await GetByUserIdQuery(userId)
            .OrderBy(s => s.NextRepetitionAt)
            .Include(e => e.VocabularyEntry)
            .ToListAsync();


        public async Task<Dictionary<int, RepetitionState>> GetDictByEntryIdsAsync(int userId, IEnumerable<int> ids)
        {
            var list = await GetByUserIdQuery(userId)
                .Where(s => ids.Contains(s.VocabularyEntryId))
                .ToListAsync();

            return list.ToDictionary(s => s.Id);
        }

        public async Task<List<VocabularyEntry>> GetEntriesWithoutRepetitionStateAsync(int userId)
        {
            return await _context.Entries.Where(e => e.User.Id == userId)
                .Where(e => e.RepetitionState == null)
                .ToListAsync();
        }
    }
}
