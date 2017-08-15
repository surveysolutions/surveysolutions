using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class DeviceInterviewersReportLine
    {
        public string TeamName { get; set; }

        public Guid TeamId { get; set; }

        public int NeverSynchedCount { get; set; }

        public int OutdatedCount { get; set; }

        public int LowStorageCount { get; set; }

        public int WrongDateOnTabletCount { get; set; }

        public int OldAndroidCount { get; set; }

        public int NeverUploadedCount { get; set; }

        public int ReassignedCount { get; set; }

        public int NoQuestionnairesCount { get; set; }
    }
}