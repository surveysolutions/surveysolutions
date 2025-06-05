using System;
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
            map.Property(b => b.Tenant);//.IsRequired();
            map.Property(b => b.Args).IsRequired();

            map.Property(b => b.Status)
                .IsRequired();
                //.HasConversionOfEnumToString();
            
            map.Property<DateTime?>(b => b.StartAt)
                .IsRequired(false)
                .HasColumnType("timestamp without time zone");
            map.Property(b => b.EndAt)
                .HasColumnType("timestamp without time zone")
                .IsRequired(false);
            map.Property(b => b.LastUpdateAt)
                .IsRequired(true)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("(now() at time zone 'utc')");
            map.Property(b => b.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("(now() at time zone 'utc')");

            map.Property<DateTime?>(p => p.ScheduleAt)
                .IsRequired(false)
                .HasColumnType("timestamp without time zone");
            
            
        }
    }
}
