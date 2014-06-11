﻿using System;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization
{
    public class SynchronizationStatus
    {
        public bool IsRunning { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? FinishedAt { get; set; }
        public DateTime? FinishedAtUtc { get; set; }
        public int ErrorsCount { get; set; }
    }
}