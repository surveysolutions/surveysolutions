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
        private const int maxActionsCount = 6;
        public int DownloadedQuestionnairesCount { get; set; }
        public int DownloadedInterviewsCount { get; set; }
        public int UploadedInterviewsCount { get; set; }
        public int NewInterviewsOnDeviceCount { get; set; }

        public int SynchronizationsWithoutChangesCount { get; set; }

        public int FailedSynchronizationsCount { get; set; }

        public bool HasAnyActivity => this.DownloadedInterviewsCount > 0 ||
                                      this.DownloadedQuestionnairesCount > 0 ||
                                      this.UploadedInterviewsCount > 0;

        public bool HasMoreThanMaxActionsCount
            => (this.DownloadedInterviewsCount + this.DownloadedQuestionnairesCount + this.UploadedInterviewsCount) > maxActionsCount;

        public int DownloadedQuestionnairesInProportionCount
            => (int) (this.DownloadedQuestionnairesCount * this.actionWeight);
        public int DownloadedInterviewsInProportionCount => (int) (this.DownloadedInterviewsCount * this.actionWeight);
        public int UploadedInterviewsInProportionCount => (int) (this.UploadedInterviewsCount * this.actionWeight);

        private int allActionsCount
            => this.DownloadedQuestionnairesCount + this.DownloadedInterviewsCount + this.UploadedInterviewsCount;

        private double actionWeight => this.HasMoreThanMaxActionsCount ? (double)maxActionsCount / allActionsCount : 1;

    }
}