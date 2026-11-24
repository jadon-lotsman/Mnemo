using Microsoft.EntityFrameworkCore;
using Mnemo.Entities;
using Mnemo.Infrastructure.Configurations;

namespace Mnemo.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<Glossary> Glossaries { get; set; }
        public DbSet<GlossaryEntry> GlossaryEntries { get; set; }
        public DbSet<GlossaryLink> GlossaryLinks { get; set; }
        public DbSet<RepetitionState> RepetitionStates { get; set; }
        public DbSet<RepetitionTask> RepetitionTasks { get; set; }
        public DbSet<User> Users { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new GlossaryConfiguration());
            modelBuilder.ApplyConfiguration(new GlossaryEntryConfiguration());
            modelBuilder.ApplyConfiguration(new GlossaryLinkConfiguration());
            
            modelBuilder.ApplyConfiguration(new RepetitionStateConfiguration());
            modelBuilder.ApplyConfiguration(new RepetitionTaskConfiguration());

            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}
