using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mnemo.Entities;

namespace Mnemo.Infrastructure.Configurations
{
    public class RepetitionStateConfiguration : IEntityTypeConfiguration<RepetitionState>
    {
        public void Configure(EntityTypeBuilder<RepetitionState> builder)
        {
            
        }
    }
}
