using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Services.Infrastructure.Storage;

namespace WB.Services.Scheduler.Model.Mapping
{
    public class JobArchiveConfiguration: IEntityTypeConfiguration<JobArchive>
    {
        public void Configure(EntityTypeBuilder<JobArchive> map)
        {
            map.ToTable("archive");

            map.HasKey(x => x.Id);
            map.Property(b => b.Id);
            map.Property(b => b.Type).IsRequired();
            map.Property(b => b.Tenant).IsRequired();
            map.Property(b => b.Args).IsRequired();

            map.Property(b => b.Status)
                .IsRequired()
                .HasConversionOfEnumToString();

            map.Property(b => b.StartAt).IsRequired(false);
            map.Property(b => b.EndAt).IsRequired(false);
            map.Property(b => b.LastUpdateAt).IsRequired(true).HasDefaultValueSql("(now() at time zone 'utc')");
            map.Property(b => b.CreatedAt).IsRequired().HasDefaultValueSql("(now() at time zone 'utc')");

            map.Property(p => p.ScheduleAt).IsRequired(false);
        }
    }
}
