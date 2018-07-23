using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.Views
{
    public class InterviewerSyncStatisticsView : IPlainStorageEntity<int?>
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        public Guid InterviewerId { get; set; }

        public int UploadedInterviewsCount { get; set; }    
        public int DownloadedInterviewsCount { get; set; }
        public int DownloadedQuestionnairesCount { get; set; }

        public int RejectedInterviewsOnDeviceCount { get; set; }
        public int NewInterviewsOnDeviceCount { get; set; }

        public int NewAssignmentsCount { get; set; }
        public int RemovedAssignmentsCount { get; set; }
        public int RemovedInterviewsCount { get; set; }

        public long TotalUploadedBytes { get; set; }
        public long TotalDownloadedBytes { get; set; }
        public double TotalConnectionSpeed { get; set; }

        public TimeSpan TotalSyncDuration { get; set; }

        public int AssignmentsOnDeviceCount { get; set; }
    }
}
