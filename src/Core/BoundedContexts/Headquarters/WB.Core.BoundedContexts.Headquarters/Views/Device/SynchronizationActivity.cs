using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Device
{
    public class SynchronizationActivity
    {
        public SyncDay[] Days { get; set; }
    }

    public class SyncDay
    {
        public string Day { get; set; }
        public SyncDayQuarter[] Quarters { get; set; }
    }

    public class SyncDayQuarter
    {
        public int DownloadedQuestionnairesCount { get; set; }
        public int DownloadedInterviewsCount { get; set; }
        public int UploadedInterviewsCount { get; set; }
        public int StartedInterviewsCount { get; set; }

        public int FailedSynchronizationsCount { get; set; }
        public int SynchronizationsWithoutChangesCount { get; set; }
    }
}