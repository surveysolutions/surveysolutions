namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync
{
    public class UploadProgress
    {
        public UploadProgress(int uploadedCount, int totalToUpload)
        {
            TotalToUpload = totalToUpload;
            UploadedCount = uploadedCount;
        }

        public int TotalToUpload { get; set; }
        public int UploadedCount { get; set; }
    }
}