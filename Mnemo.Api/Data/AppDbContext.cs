using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;

namespace Mnemo.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<VocabularyEntry> Entries { get; set; }

        public DbSet<RepetitionResult> RepetitionResults { get; set; }
        public DbSet<RepetitionState> RepetitionStates { get; set; }
        public DbSet<RepetitionTask> RepetitionTasks { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
}
