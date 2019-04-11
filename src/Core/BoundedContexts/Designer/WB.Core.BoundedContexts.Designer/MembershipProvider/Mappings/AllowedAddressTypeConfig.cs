using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Views.AllowedAddresses;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class AllowedAddressTypeConfig : IEntityTypeConfiguration<AllowedAddress> {
        public void Configure(EntityTypeBuilder<AllowedAddress> builder)
        {
            builder.ToTable("allowedaddresses", "plainstore");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Address)
                .IsRequired()
                .HasColumnName("address");

            builder.Property(e => e.Description).HasColumnName("description");
        }
    }
}
