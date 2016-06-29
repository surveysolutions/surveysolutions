namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class SychronizationStatistics
    {
        public int NewInterviewsCount { get; set; }
        public int RejectedInterviewsCount { get; set; }
        public int DeletedInterviewsCount { get; set; }
        public int SuccessfullyUploadedInterviewsCount { get; set; }

        public int TotalCompletedInterviewsCount { get; set; }
        public int TotalRejectedInterviewsCount { get; set; }
        public int TotalNewInterviewsCount { get; set; }
        public int TotalDeletedInterviewsCount { get; set; }
        public int FailedToUploadInterviwesCount { get; set; }
        public int FailedToCreateInterviewsCount { get; set; }
    }
}