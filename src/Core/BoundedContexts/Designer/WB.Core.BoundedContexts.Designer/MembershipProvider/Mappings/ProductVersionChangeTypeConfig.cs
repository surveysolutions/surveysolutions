using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.Infrastructure.Versions;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    class ProductVersionChangeTypeConfig : IEntityTypeConfiguration<ProductVersionChange>
    {
        public void Configure(EntityTypeBuilder<ProductVersionChange> builder)
        {
            builder.ToTable("productversionhistory", "plainstore");

            builder.HasKey(e => e.UpdateTimeUtc);

            builder.Property(e => e.UpdateTimeUtc)
                .HasColumnName("updatetimeutc")
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(e => e.ProductVersion).HasColumnName("productversion").IsRequired(true);
        }
    }
}
