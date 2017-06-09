﻿using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.ReadSide
{
    public class ReadSideStatus
    {
        public bool IsRebuildRunning { get; set; }
        public int ReadSideApplicationVersion { get; set; }
        public int? ReadSideDatabaseVersion { get; set; }
        public IEnumerable<ReadSideRepositoryWriterStatus> StatusByRepositoryWriters { get; set; }
        public IEnumerable<ReadSideDenormalizerStatistic> ReadSideDenormalizerStatistics { get; set; }
        public IEnumerable<ReadSideRepositoryWriterError> RebuildErrors { get; set; }
        public IEnumerable<ReadSideRepositoryWriterError> WarningEventHandlerErrors { get; set; }
        public string CurrentRebuildStatus { get; set; }
        public DateTime? LastRebuildDate { get; set; }
        public ReadSideEventPublishingDetails EventPublishingDetails { get; set; }
    }
}
