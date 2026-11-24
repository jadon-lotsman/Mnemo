using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mnemo.Entities;

namespace Mnemo.Infrastructure.Configurations
{
    public class GlossaryConfiguration : IEntityTypeConfiguration<Glossary>
    {
        public void Configure(EntityTypeBuilder<Glossary> builder)
        {
            builder.HasIndex(v => v.OwnerId);
        }
    }
}
