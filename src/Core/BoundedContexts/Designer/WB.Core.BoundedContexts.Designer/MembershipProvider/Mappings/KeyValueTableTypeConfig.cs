using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Implementation;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class KeyValueTableTypeConfig<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : KeyValueEntity
    {
        private readonly string tableName;

        public KeyValueTableTypeConfig(string tableName)
        {
            this.tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.ToTable(tableName, "plainstore");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever().IsRequired(true);
            builder.Property(x => x.Value).HasColumnName("value").HasColumnType("json").IsRequired(false);
        }
    }
}
