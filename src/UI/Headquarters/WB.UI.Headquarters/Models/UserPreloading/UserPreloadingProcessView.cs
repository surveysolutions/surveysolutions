namespace WB.UI.Headquarters.Models.UserPreloading
{
    public class UserPreloadingProcessView
    {
        public string ProcessId { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string UploadDate { get; set; }
        public string LastUpdateDate { get; set; }
        public string Status { get; set; }
        public bool CanDeleteFile { get; set; }
        public UserPreloadingProcessState State { get; set; }
    }
}