namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class UsersImportStatus
    {
        public bool IsInProgress { get; set; }
        public long UsersInQueue { get; set; }
        public long TotalUsersToImport { get; set; }
        public string FileName { get; set; }
    }
}