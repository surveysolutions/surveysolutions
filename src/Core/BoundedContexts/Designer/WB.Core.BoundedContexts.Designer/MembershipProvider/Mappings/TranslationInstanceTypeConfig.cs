using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Translations;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class TranslationInstanceTypeConfig : IEntityTypeConfiguration<TranslationInstance>
    {
        public void Configure(EntityTypeBuilder<TranslationInstance> builder)
        {
            builder.ToTable("translationinstances", "plainstore");

            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.QuestionnaireEntityId).HasColumnName("questionnaireentityid");

            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaireid");

            builder.Property(e => e.TranslationId).HasColumnName("translationid");

            builder.Property(e => e.TranslationIndex).HasColumnName("translationindex");

            builder.Property(e => e.Type).HasColumnName("type");

            builder.Property(e => e.Value).HasColumnName("value");
        }
    }
}