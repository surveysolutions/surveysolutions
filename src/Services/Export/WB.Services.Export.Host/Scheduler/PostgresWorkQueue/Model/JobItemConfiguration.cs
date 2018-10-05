using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Model
{
    public class JobItemConfiguration : IEntityTypeConfiguration<JobItem>
    {
        public void Configure(EntityTypeBuilder<JobItem> map)
        {
            map.ToTable("jobs");

            map.HasKey(x => x.Id);
            map.Property(b => b.Id);
            map.Property(b => b.Type).IsRequired().HasConversionOfEnumToString();
            map.Property(b => b.Tag).IsRequired(false);
            map.Property(b => b.Tenant).IsRequired();
            map.Property(b => b.Args).IsRequired();

            map.Property(b => b.Status)
                .IsRequired()
                .HasConversionOfEnumToString();

            map.Property(b => b.StartAt).IsRequired(false);
            map.Property(b => b.EndAt).IsRequired(false);
            map.Property(b => b.LastUpdateAt).IsRequired(true).HasDefaultValueSql("(now() at time zone 'utc')");
            map.Property(b => b.CreatedAt).IsRequired().HasDefaultValueSql("(now() at time zone 'utc')");

            map.Property(b => b.Progress).IsRequired();
            map.Property(b => b.ExportState).HasConversionOfEnumToString();
            map.Property(p => p.ErrorMessage).IsRequired(false);
            map.Property(p => p.ScheduleAt).IsRequired(false);

            map.HasIndex(nameof(JobItem.Tenant), nameof(JobItem.Status));
        }
    }
}
