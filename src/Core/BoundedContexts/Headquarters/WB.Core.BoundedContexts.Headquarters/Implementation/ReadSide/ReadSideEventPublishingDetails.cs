using System;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.ReadSide
{
    public class ReadSideEventPublishingDetails
    {
        public int ProcessedEvents { get; set; }
        public int TotalEvents { get; set; }
        public int SkippedEvents { get; set; }
        public int FailedEvents { get; set; }
        public decimal ProgressInPercents => this.TotalEvents > 0 ? Math.Round(100.00m * this.ProcessedEvents / this.TotalEvents, 2) : 0;
        public int Speed { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
    }
}