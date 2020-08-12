using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class QuestionnaireChangeReferenceTypeConfig : IEntityTypeConfiguration<QuestionnaireChangeReference>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireChangeReference> builder)
        {
            builder.ToTable("questionnairechangereferences", "plainstore");

            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.QuestionnaireChangeRecordId)
                .IsRequired(false)
                .HasColumnName("questionnairechangerecordid")
                .HasMaxLength(255);

            builder.Property(e => e.ReferenceId).HasColumnName("referenceid");

            builder.Property(e => e.ReferenceTitle).HasColumnName("referencetitle");

            builder.Property(e => e.ReferenceType).HasColumnName("referencetype");

            builder.HasOne(d => d.QuestionnaireChangeRecord)
                .WithMany(p => p.References)
                .HasForeignKey(d => d.QuestionnaireChangeRecordId)
                .HasConstraintName("questionnairechangereferences_questionnairechangerecordid");
        }
    }
}
