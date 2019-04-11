using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class QuestionnaireCompilationVersionTypeConfig : IEntityTypeConfiguration<QuestionnaireCompilationVersion>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireCompilationVersion> builder)
        {
            builder.ToTable("questionnairecompilationversions", "plainstore");
            builder.HasKey(x => x.QuestionnaireId);
            builder.Property(e => e.QuestionnaireId)
                .HasColumnName("id")
                .ValueGeneratedNever();
            builder.Property(e => e.Description).HasColumnName("description");
            builder.Property(e => e.Version).HasColumnName("version");
        }
    }
}