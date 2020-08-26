using System;

namespace WB.Services.Scheduler.Model
{
    public class JobArchive
    {
        public long Id { get; set; }
        public string Type { get; set; } = String.Empty;
        public string Args { get; set; } = "{}";
        public string? Tenant { get; set; }
        public string TenantName { get; set; } = String.Empty;
        public JobStatus Status { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdateAt { get; set; }
        public DateTime? ScheduleAt { get; set; }
    }
}