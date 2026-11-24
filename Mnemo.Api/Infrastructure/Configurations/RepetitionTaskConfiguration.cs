using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mnemo.Entities;

namespace Mnemo.Infrastructure.Configurations
{
    public class RepetitionTaskConfiguration : IEntityTypeConfiguration<RepetitionTask>
    {
        public void Configure(EntityTypeBuilder<RepetitionTask> builder)
        {
            builder.HasDiscriminator<string>("task_type")
                   .HasValue<TextRepetitionTask>("text")
                   .HasValue<OptionRepetitionTask>("option")
                   .HasValue<SentenceReorderRepetitionTask>("sentence")
                   .HasValue<SyllableReorderRepetitionTask>("syllable")
                   .HasValue<YesOrNoRepetitionTask>("yesorno");
        }
    }
}
