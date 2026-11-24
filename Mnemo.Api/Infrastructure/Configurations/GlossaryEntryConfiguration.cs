using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mnemo.Entities;
using System.Reflection.Emit;

namespace Mnemo.Infrastructure.Configurations
{
    public class GlossaryEntryConfiguration : IEntityTypeConfiguration<GlossaryEntry>
    {
        public void Configure(EntityTypeBuilder<GlossaryEntry> builder)
        {
            builder.HasIndex(e => new { e.OwnerId, e.Foreign, e.PartOfSpeech });

            builder.HasOne(e => e.RepetitionState)
                   .WithOne(s => s.Entry)
                   .HasForeignKey<RepetitionState>(s => s.EntryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
