using Microsoft.EntityFrameworkCore;
using Mnemo.Data;
using Mnemo.Data.Entities;

namespace Mnemo.Services.Queries
{
    public class SessionQueries
    {
        private AppDbContext _context;


        public SessionQueries(AppDbContext context)
        {
            _context = context;
        }


        // Queries
        public IQueryable<RepetitionSession> GetByUserIdQuery(int userId)
            => _context.RepetitionSessions.Where(s => s.UserId == userId);


        // Getters
        public async Task<bool> ExistsByUserId(int userId)
            => await GetByUserIdQuery(userId).AnyAsync();

        
        public async Task<RepetitionSession?> GetByUserIdAsync(int userId)
            => await _context.RepetitionSessions
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.User.Id == userId);

        public async Task<List<RepetitionTask>> GetTasksByUserIdAsync(int userId)
            => await _context.RepetitionTasks
            .Include(t => t.RepetitionSession)
            .Where(t => t.RepetitionSession.UserId == userId)
            .ToListAsync();

        public async Task<RepetitionTask?> GetTaskByIdAsync(int userId, int taskId)
            => await _context.RepetitionTasks
            .Include(t => t.RepetitionSession)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.RepetitionSession.UserId == userId);
    }
}
