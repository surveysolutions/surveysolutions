using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class SynchronizationStatistics
    {
        public int SuccessfullyDownloadedQuestionnairesCount { get; set; }
        public int NewInterviewsCount { get; set; }
        public int RejectedInterviewsCount { get; set; }
        public int DeletedInterviewsCount { get; set; }

        public int SuccessfullyUploadedInterviewsCount { get; set; }
        public int TotalCompletedInterviewsCount { get; set; }

        public int TotalRejectedInterviewsCount { get; set; }
        public int TotalNewInterviewsCount { get; set; }
        public int TotalDeletedInterviewsCount { get; set; }
        public int FailedToUploadInterviewsCount { get; set; }
        public int FailedToCreateInterviewsCount { get; set; }

        public int NewAssignmentsCount { get; set; }
        public int RemovedAssignmentsCount { get; set; }

        public bool NewVersionExists { get; set; }

        public int FailedInterviewsCount => this.FailedToCreateInterviewsCount + this.FailedToUploadInterviewsCount;
        public int TotalPartialDownloadedInterviewsCount { get; set; }
        public int SuccessfullyPartialDownloadedInterviewsCount { get; set; }
        public int TotalPartialUploadedInterviewsCount { get; set; }
        public int SuccessfullyPartialUploadedInterviewsCount { get; set; }
        public int FailedToPartialDownloadedInterviewsCount { get; set; }
        public int ReopenedInterviewsAfterReceivedCommentsCount { get; set; }

        public HashSet<Guid> FailToPartialProcessInterviewIds { get; set; } = new HashSet<Guid>();
        
        public int SuccessfullyUploadedCalendarEventsCount { get; set; }
        public int SuccessfullyDownloadedCalendarEventsCount { get; set; }
    }
}
