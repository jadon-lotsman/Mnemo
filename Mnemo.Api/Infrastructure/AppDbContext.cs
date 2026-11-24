using Microsoft.EntityFrameworkCore;
using Mnemo.Entities;

namespace Mnemo.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Glossary> Glossaries { get; set; }
        public DbSet<GlossaryLink> GlossaryLinks { get; set; }
        public DbSet<GlossaryEntry> GlossaryEntries { get; set; }
        public DbSet<RepetitionTask> RepetitionTasks { get; set; }
        public DbSet<RepetitionState> RepetitionStates { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Indexes
            modelBuilder.Entity<Glossary>()
                .HasIndex(v => v.OwnerId);

            modelBuilder.Entity<GlossaryEntry>()
                .HasIndex(e => new { e.OwnerId, e.Foreign, e.PartOfSpeech });

            modelBuilder.Entity<GlossaryLink>()
                .HasIndex(l => l.EntryId);

            modelBuilder.Entity<RepetitionState>()
                .HasIndex(s => s.EntryId)
                .IsUnique();

            // Relations
            modelBuilder.Entity<User>()
                .HasMany(p => p.Glossaries)
                .WithOne(u => u.Owner)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(p => p.RepetitionTasks)
                .WithOne(u => u.Owner)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<GlossaryLink>()
                .HasKey(l => new { l.GlossaryId, l.EntryId });

            modelBuilder.Entity<GlossaryLink>()
                .HasOne(v => v.Glossary)
                .WithMany(l => l.Links)
                .HasForeignKey(v => v.GlossaryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GlossaryLink>()
                .HasOne(e => e.Entry)
                .WithMany(l => l.Links)
                .HasForeignKey(e => e.EntryId);

            modelBuilder.Entity<GlossaryEntry>()
                .HasOne(e => e.RepetitionState)
                .WithOne(s => s.Entry)
                .HasForeignKey<RepetitionState>(s => s.EntryId)
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
