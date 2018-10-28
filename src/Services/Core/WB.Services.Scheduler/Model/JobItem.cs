using System;
using System.Collections.Generic;
using WB.Services.Scheduler.Model.Events;

namespace WB.Services.Scheduler.Model
{
    public class JobItem
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Args { get; set; } = "{}";
        public string Tag { get; set; }
        public string Tenant { get; set; }
        public string TenantName { get; set; }
        public JobStatus Status { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdateAt { get; set; }
        public DateTime? ScheduleAt { get; set; }
        public string WorkerId { get; set; }

        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public T GetData<T>(string key) => Data.TryGetValue(key, out var val) ? (T) val : default;

        public void Handle(IJobEvent @event)
        {
            this.Apply((dynamic)@event);
            this.LastUpdateAt = DateTime.UtcNow;
        }

        private void Apply(StartJobEvent @event)
        {
            this.StartAt = DateTime.UtcNow;
            this.Status = JobStatus.Running;
            this.WorkerId = @event.WorkerId;
        }

        private void Apply(CancelJobEvent ev)
        {
            this.EndAt = DateTime.UtcNow;
            this.Status = JobStatus.Canceled;
            this.Data["error"] = ev.Reason;
        }

        private void Apply(CompleteJobEvent ev)
        {
            this.EndAt = DateTime.UtcNow;
            this.Status = JobStatus.Completed;
        }

        private void Apply(FailJobEvent ev)
        {
            this.EndAt = DateTime.UtcNow;
            this.Status = JobStatus.Fail;
            this.Data["error"] = ev.Exception.ToString();
        }

        private void Apply(UpdateDataEvent ev)
        {
            this.Data[ev.Key] = ev.Value;
        }

        public JobItem Start(string workerId)
        {
            this.Handle(new StartJobEvent(Id, workerId));
            return this;
        }

        public JobItem Cancel(string reason)
        {
            this.Handle(new CancelJobEvent(Id, reason));
            return this;
        }
    }
}
