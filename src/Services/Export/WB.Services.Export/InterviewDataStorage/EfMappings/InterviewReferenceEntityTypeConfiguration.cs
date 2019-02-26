using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WB.Services.Export.InterviewDataStorage.EfMappings
{
    public class InterviewReferenceEntityTypeConfiguration : IEntityTypeConfiguration<InterviewReference>
    {
        public void Configure(EntityTypeBuilder<InterviewReference> builder)
        {
            builder.ToTable("interview__references");
            builder.HasKey(x => x.InterviewId);
        }
    }

    public class DeletedQuestionnaireReferenceTypeConfiguration : IEntityTypeConfiguration<GeneratedQuestionnaireReference>
    {
        public void Configure(EntityTypeBuilder<GeneratedQuestionnaireReference> builder)
        {
            builder.ToTable("__generated_questionnaire_reference");
            builder.HasKey(x => x.Id);
            builder.Property(_ => _.DeletedAt);
        }
    }
}
