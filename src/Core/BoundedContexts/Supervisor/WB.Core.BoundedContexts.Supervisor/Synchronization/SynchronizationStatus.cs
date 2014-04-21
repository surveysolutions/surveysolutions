using System;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class SynchronizationStatus
    {
        public bool IsRunning { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public bool? IsHqReachable { get; set; }
        public DateTime? FinisedAt { get; set; }
        public DateTime? FinishedAtUtc { get; set; }
        public int ErrorsCount { get; set; }
    }
}