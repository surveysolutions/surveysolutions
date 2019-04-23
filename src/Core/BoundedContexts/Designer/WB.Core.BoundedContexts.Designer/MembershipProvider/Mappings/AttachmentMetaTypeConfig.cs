using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class AttachmentMetaTypeConfig : IEntityTypeConfiguration<AttachmentMeta>
    {
        public void Configure(EntityTypeBuilder<AttachmentMeta> builder)
        {
            builder.ToTable("attachmentmetas", "plainstore");

            builder.HasKey(e => e.AttachmentId);

            builder.Property(e => e.AttachmentId)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.ContentId).HasColumnName("contentid");

            builder.Property(e => e.FileName).HasColumnName("filename");

            builder.Property(e => e.LastUpdateDate).HasColumnName("lastupdatedate");

            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaireid");
        }
    }
}