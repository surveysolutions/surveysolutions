using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.Implementation.ReadSide
{
    public class ReadSideStatus
    {
        public bool IsRebuildRunning { get; set; }
        public IEnumerable<ReadSideRepositoryWriterStatus> StatusByRepositoryWriters { get; set; }
        public IEnumerable<ReadSideDenormalizerStatistic> ReadSideDenormalizerStatistics { get; set; }
        public IEnumerable<ReadSideRepositoryWriterError> RebuildErrors { get; set; }
        public string CurrentRebuildStatus { get; set; }
        public DateTime LastRebuildDate { get; set; }
        public ReadSideEventPublishingDetails EventPublishingDetails { get; set; }
    }
}
