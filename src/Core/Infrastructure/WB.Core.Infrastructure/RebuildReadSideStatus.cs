using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure
{
    public class RebuildReadSideStatus
    {
        public bool IsRebuildRunning { get; set; }
        public IEnumerable<ReadSideRepositoryWriterStatus> StatusByRepositoryWriters { get; set; }
        public IEnumerable<ReadSideRepositoryWriterError> RebuildErrors { get; set; }
        public string CurrentRebuildStatus { get; set; }
        public DateTime LastRebuildDate { get; set; }
        public RebuildReadSideEventPublishingDetails EventPublishingDetails { get; set; }
    }
}
