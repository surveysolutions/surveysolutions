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
        public int UploadedInterviewsCount { get; set; }
        public int DownloadedAssigmentsCount { get; set; }
        public int AllAssignmentsOnDeviceCount { get; set; }

        public int SynchronizationsWithoutChangesCount { get; set; }

        public int FailedSynchronizationsCount { get; set; }

        public bool HasAnyActivity => this.DownloadedAssigmentsCount > 0 ||
                                      this.UploadedInterviewsCount > 0;

        public bool HasMoreThanMaxActionsCount
            => (this.DownloadedAssigmentsCount + this.UploadedInterviewsCount) > maxActionsCount;

        public int DownloadedAssignmentsInProportionCount 
            => this.NormalizeProportion(this.DownloadedAssigmentsCount * this.actionWeight);

        public int UploadedInterviewsInProportionCount 
            => this.NormalizeProportion(this.UploadedInterviewsCount * this.actionWeight);

        private int NormalizeProportion(double actionsProportion)
        {
            if (actionsProportion > 0 && actionsProportion < 1)
                return 1;

            return (int)actionsProportion;
        }

        private int allActionsCount
            => this.DownloadedAssigmentsCount + this.UploadedInterviewsCount;

        private double actionWeight => this.HasMoreThanMaxActionsCount ? (double)maxActionsCount / allActionsCount : 1;

    }
}