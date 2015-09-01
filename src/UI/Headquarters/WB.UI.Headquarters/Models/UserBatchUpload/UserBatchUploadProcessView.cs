using WB.UI.Headquarters.API;

namespace WB.UI.Headquarters.Models.UserBatchUpload
{
    public class UserBatchUploadProcessView
    {
        public string ProcessId { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string UploadDate { get; set; }
        public string LastUpdateDate { get; set; }
        public string Status { get; set; }
        public bool CanDeleteFile { get; set; }
        public UserBatchUploadProcessState State { get; set; }
    }
}