using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;

namespace Mnemo.Data.Queries
{
    public class AccountQueries
    {
        private AppDbContext _context;


        public AccountQueries(AppDbContext context)
        {
            _context = context;
        }


        // Getters
        public async Task<bool> ExistsByIdAsync(int userId)
            => await _context.Users.AnyAsync(u => u.Id == userId);

        public async Task<bool> ExistsByUsernameAsync(string username)
            => await _context.Users.AnyAsync(u => u.Username == username);


        public async Task<User?> GetByIdAsync(int userId)
            => await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        public async Task<User?> GetByUsernameAsync(string username)
            => await _context.Users.FirstOrDefaultAsync(u => string.Equals(u.Username, username));
    }
}
