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
            builder.ToTable("anonymousquestionnaires", "plainstore");

            builder.HasKey(x => x.AnonymousQuestionnaireId);

            builder.Property(e => e.AnonymousQuestionnaireId)
                .HasColumnName("anonymousquestionnaireid").ValueGeneratedNever();

            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaireid");

            builder.Property(e => e.IsActive).HasColumnName("isactive");
        }
    }
}