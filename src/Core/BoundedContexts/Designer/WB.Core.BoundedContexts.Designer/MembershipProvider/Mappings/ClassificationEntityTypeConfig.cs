using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Classifications;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    class ClassificationEntityTypeConfig : IEntityTypeConfiguration<ClassificationEntity>
    {
        public void Configure(EntityTypeBuilder<ClassificationEntity> builder)
        {
            builder.ToTable("classificationentities", "plainstore");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.ClassificationId).HasColumnName("classificationid");

            builder.Property(e => e.Index).HasColumnName("index");

            builder.Property(e => e.Parent).HasColumnName("parent").IsRequired(false);

            builder.Property(e => e.Title)
                .IsRequired()
                .HasColumnName("title");

            builder.Property(e => e.Type).HasColumnName("type");

            builder.Property(e => e.UserId).HasColumnName("userid");

            builder.Property(e => e.Value).HasColumnName("value");
        }
    }
}
