using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;

namespace Mnemo.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Vocabulary> Vocabularies { get; set; }
        public DbSet<VocabularyEntry> VocabularyEntries { get; set; }
        public DbSet<VocabularyEntryLink> VocabularyEntryLinks { get; set; }
        public DbSet<RepetitionTask> RepetitionTasks { get; set; }
        public DbSet<RepetitionState> RepetitionStates { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Indexes
            modelBuilder.Entity<Vocabulary>()
                .HasIndex(v => v.OwnerId);

            modelBuilder.Entity<VocabularyEntry>()
                .HasIndex(e => new { e.OwnerId, e.Foreign, e.PartOfSpeech });
            modelBuilder.Entity<VocabularyEntry>()
                .HasIndex(e => new { e.OwnerId, e.MergedFromId });

            modelBuilder.Entity<VocabularyEntryLink>()
                .HasIndex(l => l.VocabularyEntryId);

            modelBuilder.Entity<RepetitionState>()
                .HasIndex(s => s.VocabularyEntryId)
                .IsUnique();

            // Relations
            modelBuilder.Entity<User>()
                .HasMany(p => p.Vocabularies)
                .WithOne(u => u.Owner)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(p => p.RepetitionTasks)
                .WithOne(u => u.Owner)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<VocabularyEntryLink>()
                .HasKey(l => new { l.VocabularyId, l.VocabularyEntryId });

            modelBuilder.Entity<VocabularyEntryLink>()
                .HasOne(v => v.Vocabulary)
                .WithMany(l => l.EntryLinks)
                .HasForeignKey(v => v.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VocabularyEntryLink>()
                .HasOne(e => e.VocabularyEntry)
                .WithMany(l => l.VocabularyLinks)
                .HasForeignKey(e => e.VocabularyEntryId);

            modelBuilder.Entity<VocabularyEntry>()
                .HasOne(e => e.RepetitionState)
                .WithOne(s => s.VocabularyEntry)
                .HasForeignKey<RepetitionState>(s => s.VocabularyEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Discriminators
            modelBuilder.Entity<RepetitionTask>()
                .HasDiscriminator<string>("task_type")
                .HasValue<TextRepetitionTask>("text")
                .HasValue<OptionRepetitionTask>("option")
                .HasValue<SentenceReorderRepetitionTask>("sentence")
                .HasValue<SyllableReorderRepetitionTask>("syllable")
                .HasValue<YesOrNoRepetitionTask>("yesorno");
        }
    }
}
