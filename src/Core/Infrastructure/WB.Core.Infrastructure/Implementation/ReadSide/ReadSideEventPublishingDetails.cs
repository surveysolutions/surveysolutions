using System;

namespace WB.Core.Infrastructure.Implementation.ReadSide
{
    public class ReadSideEventPublishingDetails
    {
        public int ProcessedEvents { get; set; }
        public int TotalEvents { get; set; }
        public int SkippedEvents { get; set; }
        public int FailedEvents { get; set; }
        public decimal ProgressInPercents => TotalEvents > 0 ? Math.Round(100.00m * ProcessedEvents / TotalEvents, 2) : 0;
        public int Speed { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
    }
}