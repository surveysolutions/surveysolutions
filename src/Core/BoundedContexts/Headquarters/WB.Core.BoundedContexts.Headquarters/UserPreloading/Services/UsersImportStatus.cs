using System;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class UsersImportStatus
    {
        public bool IsInProgress { get; set; }
        public long UsersInQueue { get; set; }
        public long TotalUsersToImport { get; set; }
        public string FileName { get; set; }
        public bool IsOwnerOfRunningProcess { get; set; }
        public string Responsible { get; set; }
        public DateTime? StartedDate { get; set; }
    }
}
