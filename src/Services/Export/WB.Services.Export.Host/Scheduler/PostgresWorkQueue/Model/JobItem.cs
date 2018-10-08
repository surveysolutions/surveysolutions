using System;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Model
{
    public class JobItem
    {
        public long Id { get; set; }
        public JobType Type { get; set; }
        public string Args { get; set; } = "{}";
        public string Tag { get; set; }
        public string Tenant { get; set; }
        public JobStatus Status { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdateAt { get; set; }
        public DateTime? ScheduleAt { get; set; }

        public int Progress { get; set; }
        
        public DataExportStatus ExportState { get; set; }
        public string ErrorMessage { get; set; }
    }
}
