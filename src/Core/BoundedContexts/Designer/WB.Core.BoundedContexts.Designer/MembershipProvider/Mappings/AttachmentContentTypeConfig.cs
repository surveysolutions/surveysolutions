using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class AttachmentContentTypeConfig : IEntityTypeConfiguration<AttachmentContent>
    {
        public void Configure(EntityTypeBuilder<AttachmentContent> builder)
        {
            builder.ToTable("attachmentcontents", "plainstore");

            builder.HasKey(x => x.ContentId);
            builder.Property(e => e.ContentId)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.OwnsOne(x => x.Details, p =>
            {
                p.Property(e => e.Height).HasColumnName("attachmentheight");

                p.Property(e => e.Width).HasColumnName("attachmentwidth");

                p.Property(e => e.Thumbnail).HasColumnName("thumbnail");
            });
            

            builder.Property(e => e.Content).HasColumnName("content");

            builder.Property(e => e.ContentType).HasColumnName("contenttype");

            builder.Property(e => e.Size).HasColumnName("size");
        }
    }
}
