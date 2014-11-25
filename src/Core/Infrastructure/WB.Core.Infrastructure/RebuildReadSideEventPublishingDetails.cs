using System;

namespace WB.Core.Infrastructure
{
    public class RebuildReadSideEventPublishingDetails
    {
        public int ProcessedEvents { get; set; }
        public int TotalEvents { get; set; }
        public int SkippedEvents { get; set; }
        public int FailedEvents { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public int Speed { get; set; }
        public TimeSpan EstimatedTime { get; set; }
    }
}