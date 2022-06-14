using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class AnonymousQuestionnaireTypeConfig : IEntityTypeConfiguration<AnonymousQuestionnaire>
    {
        public void Configure(EntityTypeBuilder<AnonymousQuestionnaire> builder)
        {
            builder.ToTable("anonymous_questionnaires", "plainstore");

            builder.HasKey(x => x.AnonymousQuestionnaireId);

            builder.Property(e => e.AnonymousQuestionnaireId)
                .HasColumnName("anonymous_questionnaire_id").ValueGeneratedNever();

            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaire_id");

            builder.Property(e => e.IsActive).HasColumnName("is_active");
            builder.Property(e => e.GeneratedAtUtc).HasColumnName("generated_at_utc");
        }
    }
}