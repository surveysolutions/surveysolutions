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
            
            builder.Property(e => e.QuestionnaireId).IsRequired(false);
            //builder.Property(e => e.InterviewId).IsRequired(true);
            builder.Property(e => e.Status).IsRequired(true);
            builder.Property(e => e.Key).IsRequired(false);
            builder.Property(e => e.UpdateDateUtc)
                .IsRequired(false)
                .HasColumnType("timestamp without time zone");
            builder.Property(e => e.DeletedAtUtc)
                .IsRequired(false)
                .HasColumnType("timestamp without time zone");
            builder.Property(e => e.AssignmentId).IsRequired(false);
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

            builder.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .IsRequired(false);
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
            
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Value).IsRequired(false);
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
            builder.Property(a => a.ResponsibleId).IsRequired(true);
            builder.Property(a => a.Quantity).IsRequired(false);
            builder.Property(a => a.AudioRecording).IsRequired(true);
            builder.Property(a => a.WebMode).IsRequired(false);
            builder.Property(a => a.Comment).IsRequired(false);
            builder.Property(a => a.QuestionnaireId).IsRequired(false);
            builder.Property(a => a.UpgradedFromId).IsRequired(false);
            builder.Property(a => a.TargetArea).IsRequired(false);
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

            //builder.Property(a => a.GlobalSequence).IsRequired(true);
            //builder.Property(a => a.Position).IsRequired(true);
            //builder.Property(a => a.AssignmentId).IsRequired(true);
            builder.Property(a => a.Status).IsRequired(true);
            builder.Property(a => a.TimestampUtc).IsRequired(true)
                .HasColumnType("timestamp without time zone");;
            builder.Property(a => a.OriginatorId).IsRequired(true);
            builder.Property(a => a.ResponsibleId).IsRequired(true);
            builder.Property(a => a.OldValue).IsRequired(false);
            builder.Property(a => a.NewValue).IsRequired(false);
            builder.Property(a => a.Comment).IsRequired(false);
        }
    }
}
