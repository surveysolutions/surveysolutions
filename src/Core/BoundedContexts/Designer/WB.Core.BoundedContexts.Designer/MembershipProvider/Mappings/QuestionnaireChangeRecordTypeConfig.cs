using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class QuestionnaireChangeRecordTypeConfig : IEntityTypeConfiguration<QuestionnaireChangeRecord>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireChangeRecord> builder)
        {
            builder.ToTable("questionnairechangerecords", "plainstore");

            builder.HasKey(x => x.QuestionnaireChangeRecordId);

            builder.Property(e => e.QuestionnaireChangeRecordId)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.ActionType).HasColumnName("actiontype");

            builder.Property(e => e.AffectedEntriesCount).HasColumnName("affectedentriescount").IsRequired(false);

            builder.Property(e => e.Patch).HasColumnName("patch").IsRequired(false);

            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaireid");


            builder.Property(e => e.ResultingQuestionnaireDocument).HasColumnName("resultingquestionnairedocument").IsRequired(false);

            builder.Property(e => e.Sequence).HasColumnName("sequence");

            builder.Property(e => e.TargetItemDateTime).HasColumnName("targetitemdatetime").IsRequired(false);

            builder.Property(e => e.TargetItemId).HasColumnName("targetitemid");

            builder.Property(e => e.TargetItemNewTitle).HasColumnName("targetitemnewtitle").IsRequired(false);

            builder.Property(e => e.TargetItemTitle).HasColumnName("targetitemtitle").IsRequired(false);

            builder.Property(e => e.TargetItemType).HasColumnName("targetitemtype");

            builder.Property(e => e.Timestamp).HasColumnName("timestamp");

            builder.Property(e => e.UserId).HasColumnName("userid");

            builder.Property(e => e.UserName).HasColumnName("username").IsRequired(false);

            builder.Property(e => e.Meta).HasColumnName("meta")
                .IsRequired(false)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<QuestionnaireChangeRecordMetadata>(v));                
        }
    }
}
