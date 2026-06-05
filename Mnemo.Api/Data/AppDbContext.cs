using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;

namespace Mnemo.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<VocabularyEntry> Entries { get; set; }

        public DbSet<RepetitionState> RepetitionStates { get; set; }
        public DbSet<RepetitionTask> RepetitionTasks { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VocabularyEntry>()
                .HasOne(e => e.RepetitionState)
                .WithOne(s => s.VocabularyEntry)
                .HasForeignKey<RepetitionState>(s => s.VocabularyEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VocabularyEntry>()
                .HasOne(e => e.User)
                .WithMany(u => u.VocabularyEntries)
                .HasForeignKey(e => e.UserId);


            modelBuilder.Entity<RepetitionTask>()
                .HasDiscriminator<string>("task_type")
                .HasValue<TextRepetitionTask>("text")
                .HasValue<OptionRepetitionTask>("option")
                .HasValue<OrderPartsRepetitionTask>("parts")
                .HasValue<YesOrNoRepetitionTask>("yesorno");
        }
    }
}
