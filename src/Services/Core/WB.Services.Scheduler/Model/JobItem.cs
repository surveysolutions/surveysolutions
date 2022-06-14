using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using WB.Services.Scheduler.Model.Events;

namespace WB.Services.Scheduler.Model
{
    public class JobItem
    {
        public long Id { get; set; }
        public string Type { get; set; } = String.Empty;
        public string Args { get; set; } = "{}";
        public string Tag { get; set; } = String.Empty;
        public string? Tenant { get; set; }
        public string TenantName { get; set; } = String.Empty;
        public JobStatus Status { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdateAt { get; set; }
        public DateTime? ScheduleAt { get; set; }
        public string? WorkerId { get; set; }

        public Dictionary<string, object?>? Data { get; set; } = new Dictionary<string, object?>();

        public T? GetData<T>(string key) where T: class => Data == null ? default : Data.TryGetValue(key, out var val) ? (T?)val : default;

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
            this.Error = ev.Reason;
            this.ErrorType = Enum.GetName(typeof(JobError), JobError.Canceled);
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

        [NotMapped]
        public long FailedTimes
        {
            get => (long)(this["failedTimes"] ?? 0L);
            set => this["failedTimes"] = value;
        }

        private string? Error
        {
            get => (string?)this["error"];
            set => this["error"] = value;
        }

        private string? ErrorType
        {
            get => (string?)this["errorType"];
            set => this["errorType"] = value;
        }

        [NotMapped]
        public long MaxRetryAttempts
        {
            get => (long) (this["maxRetry"] ?? 3L);
            set => this["maxRetry"] = value;
        } 

        public object? this[string key]
        {
            get => Data == null ? null : Data.TryGetValue(key, out var res) ? res : null;
            set { if (Data != null) Data[key] = value; }
        }

        private void Apply(FailJobEvent ev)
        {
            switch (ev.Exception)
            {
                case IOException io when io.HResult == 0x70:
                    this.ErrorType = JobError.NotEnoughExternalStorageSpace.ToString();
                    this.Error = io.ToStringDemystified();
                    this.Status = JobStatus.Fail;
                    return;
                default:
                    if (FailedTimes++ < MaxRetryAttempts)
                    {
                        this.Status = JobStatus.Created;
                        this.ScheduleAt = DateTime.UtcNow.AddSeconds(10);
                        return;
                    }

                    break;
            }

            this.EndAt = DateTime.UtcNow;
            this.Status = JobStatus.Fail;
            this.Error = ev.Exception.ToStringDemystified();
            this.ErrorType = JobError.Unexpected.ToString();
        }

        private void Apply(UpdateDataEvent ev)
        {
            if (Data != null) Data[ev.Key] = ev.Value;
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

        // drop tenant schema before last retry
        public bool ShouldDropTenantSchema => false; //this.FailedTimes == MaxRetryAttempts;
    }
}
