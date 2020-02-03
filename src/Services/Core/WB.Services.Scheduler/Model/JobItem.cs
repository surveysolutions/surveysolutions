using System;
using System.Collections.Generic;
using System.IO;
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
            this.Data["errorType"] = Enum.GetName(typeof(JobError), JobError.Canceled);
        }

        private void Apply(CompleteJobEvent _)
        {
            this.EndAt = DateTime.UtcNow;
            this.Status = JobStatus.Completed;
        }

        private void Apply(ReEnqueueJobEvent _)
        {
            this.Status = JobStatus.Created;
            this.EndAt = null;
        }

        private void Apply(FailJobEvent ev)
        {
            this.EndAt = DateTime.UtcNow;
            
            if (!this.Data.ContainsKey("retry"))
            {
                this.Data["retry"] = 3l;
            }

            var retryLeft = this.GetData<long>("retry");
            if (retryLeft > 0)
            {
                this.Status = JobStatus.Created;
                this.Data["retry"] = retryLeft - 1;
            }
            else
            {
                this.Status = JobStatus.Fail;
                this.Data["error"] = ev.Exception.ToString();

                var errorType = JobError.Unexpected;
                switch (ev.Exception)
                {
                    case IOException io when io.HResult == 0x70:
                        errorType = JobError.NotEnoughExternalStorageSpace;
                        break;
                }

                this.Data["errorType"] = Enum.GetName(typeof(JobError), errorType);
            }
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

        public JobItem ReEnqueue()
        {
            this.Handle(new ReEnqueueJobEvent(Id));
            return this;
        }

        public JobItem Cancel(string reason)
        {
            this.Handle(new CancelJobEvent(Id, reason));
            return this;
        }
    }
}
