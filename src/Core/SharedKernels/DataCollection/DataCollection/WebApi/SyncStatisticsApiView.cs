namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class SyncStatisticsApiView
    {
        public int UploadedInterviewsCount { get; set; }
        public int DownloadedInterviewsCount { get; set; }
        public int DownloadedQuestionnairesCount { get; set; }

        public int RejectedInterviewsOnDeviceCount { get; set; }
        public int NewInterviewsOnDeviceCount { get; set; }
    }
}