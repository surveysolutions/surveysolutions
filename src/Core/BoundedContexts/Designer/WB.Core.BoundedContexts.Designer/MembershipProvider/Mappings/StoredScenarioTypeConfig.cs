using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Scenarios;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    class StoredScenarioTypeConfig : IEntityTypeConfiguration<StoredScenario>
    {
        public void Configure(EntityTypeBuilder<StoredScenario> builder)
        {
            builder.ToTable("stored_scenarios", "plainstore");
            builder.HasKey(k => k.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(x => x.QuestionnaireId).HasColumnName("questionnaire_id");
            builder.Property(x => x.Steps).HasColumnName("steps");
            builder.Property(x => x.Title).HasColumnName("title");
        }
    }
}