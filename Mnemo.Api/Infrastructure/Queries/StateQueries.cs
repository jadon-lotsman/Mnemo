using Microsoft.EntityFrameworkCore;
using Mnemo.Entities;

namespace Mnemo.Infrastructure.Queries
{
    public class StateQueries
    {
        private AppDbContext _context;


        public StateQueries(AppDbContext context)
        {
            _context = context;
        }


        // Queries
        public IQueryable<RepetitionState> GetByUserIdQuery(int userId)
            => _context.RepetitionStates.Where(rs => rs.Entry.OwnerId == userId);


        // Getters
        public async Task<RepetitionState?> GetByEntryIdAsync(int userId, Guid entryId)
            => await GetByUserIdQuery(userId).FirstOrDefaultAsync(s => s.EntryId == entryId);

        public async Task<Dictionary<int, RepetitionState>> GetDictByEntryIdsAsync(int userId, IEnumerable<Guid> ids)
        {
            var list = await GetByUserIdQuery(userId)
                .Where(s => ids.Contains(s.EntryId))
                .ToListAsync();

            return list.ToDictionary(s => s.Id);
        }
    }
}
