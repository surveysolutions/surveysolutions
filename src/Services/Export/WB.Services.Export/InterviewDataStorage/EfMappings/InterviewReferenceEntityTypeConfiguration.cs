using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Services.Export.Assignment;

namespace WB.Services.Export.InterviewDataStorage.EfMappings
{
    public class InterviewReferenceEntityTypeConfiguration : IEntityTypeConfiguration<InterviewReference>
    {
        private readonly string schema;

        public InterviewReferenceEntityTypeConfiguration(string schema)
        {
            this.schema = schema;
        }

        public void Configure(EntityTypeBuilder<InterviewReference> builder)
        {
            builder.ToTable("interview__references", schema);
            builder.HasKey(x => x.InterviewId);
        }
    }

    public class DeletedQuestionnaireReferenceTypeConfiguration : IEntityTypeConfiguration<GeneratedQuestionnaireReference>
    {
        private readonly string schema;

        public DeletedQuestionnaireReferenceTypeConfiguration(string schema)
        {
            this.schema = schema;
        }

        public void Configure(EntityTypeBuilder<GeneratedQuestionnaireReference> builder)
        {
            builder.ToTable("__generated_questionnaire_reference", schema);
            builder.HasKey(x => x.Id);
            builder.Property(_ => _.DeletedAt);
        }
    }

    public class MetadataTypeConfiguration : IEntityTypeConfiguration<Metadata>
    {
        private readonly string schema;

        public MetadataTypeConfiguration(string schema)
        {
            this.schema = schema;
        }

        public void Configure(EntityTypeBuilder<Metadata> builder)
        {
            builder.ToTable("metadata", schema);
        }
    }

    public class AssignmentTypeConfiguration : IEntityTypeConfiguration<Assignment.Assignment>
    {
        private readonly string schema;

        public AssignmentTypeConfiguration(string schema)
        {
            this.schema = schema;
        }

        public void Configure(EntityTypeBuilder<Assignment.Assignment> builder)
        {
            builder.ToTable("assignment", schema);
        }
    }

    public class AssignmentActionTypeConfiguration : IEntityTypeConfiguration<AssignmentAction>
    {
        private readonly string schema;

        public AssignmentActionTypeConfiguration(string schema)
        {
            this.schema = schema;
        }

        public void Configure(EntityTypeBuilder<AssignmentAction> builder)
        {
            builder.ToTable("assignment__action", schema);
        }
    }
}
