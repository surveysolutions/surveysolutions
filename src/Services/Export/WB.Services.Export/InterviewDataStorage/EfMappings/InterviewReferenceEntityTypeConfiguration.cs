using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Services.Export.Assignment;

namespace WB.Services.Export.InterviewDataStorage.EfMappings
{
    public class InterviewReferenceEntityTypeConfiguration : IEntityTypeConfiguration<InterviewReference>
    {
        private readonly string? schema;

        public InterviewReferenceEntityTypeConfiguration(string? schema)
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
        private readonly string? schema;

        public DeletedQuestionnaireReferenceTypeConfiguration(string? schema)
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
        private readonly string? schema;

        public MetadataTypeConfiguration(string? schema)
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
        private readonly string? schema;

        public AssignmentTypeConfiguration(string? schema)
        {
            this.schema = schema;
        }

        public void Configure(EntityTypeBuilder<Assignment.Assignment> builder)
        {
            builder.ToTable("__assignment", schema);
            builder.HasKey(a => a.PublicKey);
            builder.HasAlternateKey(a => a.Id);

            builder.Property(a => a.Id).ValueGeneratedNever();
            builder.Property(a => a.PublicKey).ValueGeneratedNever();
            builder.Property(a => a.WebMode).IsRequired(false);
            builder.Property(a => a.AudioRecording).IsRequired(true);
            builder.Property(a => a.Quantity).IsRequired(false);
            builder.Property(a => a.Comment).IsRequired(false);
            builder.Property(a => a.UpgradedFromId).IsRequired(false);
        }
    }

    public class AssignmentActionTypeConfiguration : IEntityTypeConfiguration<AssignmentAction>
    {
        private readonly string? schema;

        public AssignmentActionTypeConfiguration(string? schema)
        {
            this.schema = schema;
        }

        public void Configure(EntityTypeBuilder<AssignmentAction> builder)
        {
            builder.ToTable("__assignment__action", schema);
            builder.HasKey(aa => new { aa.GlobalSequence, aa.Position });
            builder.HasIndex(aa => aa.AssignmentId);

            builder.Property(a => a.OldValue).IsRequired(false);
            builder.Property(a => a.NewValue).IsRequired(false);
            builder.Property(a => a.Comment).IsRequired(false);

        }
    }
}
