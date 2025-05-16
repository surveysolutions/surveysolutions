using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using WB.Services.Infrastructure.Storage;

namespace WB.Services.Scheduler.Model.Mapping
{
    public class JobItemConfiguration : IEntityTypeConfiguration<JobItem>
    {
        public void Configure(EntityTypeBuilder<JobItem> map)
        {
            map.ToTable("jobs");

            map.HasKey(x => x.Id);
            map.Property(b => b.Id);
            map.Property(b => b.Type).IsRequired();
            map.Property(b => b.Tag).IsRequired(true);
            map.Property(b => b.Tenant).IsRequired(false);
            map.Property(b => b.Args).IsRequired();

            map.Property(b => b.Status)
                .IsRequired()
                .HasConversionOfEnumToString();

            map.Property(b => b.StartAt)
                .IsRequired(false)
                .HasColumnType("timestamp without time zone");
            map.Property(b => b.EndAt)
                .IsRequired(false)
                .HasColumnType("timestamp without time zone");
            map.Property(b => b.LastUpdateAt)
                .IsRequired(true)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("(now() at time zone 'utc')");
            map.Property(b => b.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("(now() at time zone 'utc')");

            map.Property(b => b.Data)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, object?>>(v))
                .IsRequired(true);
            map.Property(p => p.ScheduleAt)
                .IsRequired(false)
                .HasColumnType("timestamp without time zone");

            map.HasIndex(nameof(JobItem.Tenant), nameof(JobItem.Status));
            map.HasIndex(ji => new {ji.Type, ji.Status, ji.Tenant});
        }
    }
}
