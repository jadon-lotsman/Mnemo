using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mnemo.Entities;
using System.Reflection.Emit;

namespace Mnemo.Infrastructure.Configurations
{
    public class GlossaryLinkConfiguration : IEntityTypeConfiguration<GlossaryLink>
    {
        public void Configure(EntityTypeBuilder<GlossaryLink> builder)
        {
            builder.HasKey(l => new { l.GlossaryId, l.EntryId });

            builder.HasIndex(l => l.EntryId);

            builder.HasOne(e => e.Entry)
                   .WithMany(l => l.Links)
                   .HasForeignKey(e => e.EntryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Glossary)
                   .WithMany(l => l.Links)
                   .HasForeignKey(v => v.GlossaryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
