using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class DeviceInterviewersReportLine
    {
        public string TeamName { get; set; }

        public Guid TeamId { get; set; }

        public int NeverSynchedCount { get; set; }

        public int OutdatedCount { get; set; }
    }
}