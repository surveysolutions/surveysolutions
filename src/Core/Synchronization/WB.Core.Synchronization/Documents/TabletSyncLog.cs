using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.Documents
{
    public class TabletSyncLog
    {
        public TabletSyncLog()
        {
            this.RegisteredUsersOnDevice = new HashSet<Guid>();
            this.UserSyncLog = new List<TabletSyncLogByUser>();
        }

        public virtual string DeviceId { get; set; }

        public virtual string AndroidId { get; set; }

        public virtual DateTime RegistrationDate { get; set; }

        public virtual DateTime LastUpdateDate { get; set; }

        public virtual ISet<Guid> RegisteredUsersOnDevice { get; set; }

        public virtual IList<TabletSyncLogByUser> UserSyncLog { get; set; }
    }

    public class TabletSyncLogByUser
    {
        public TabletSyncLogByUser()
        {
            this.PackagesTrackingInfo = new List<SyncPackageTrackingInfo>();
        }
        public virtual int Id { get; set; }

        public virtual DateTime HandshakeTime { get; set; }

        public virtual string AppVersion { get; set; }

        public virtual string UserId { get; set; }

        public virtual IList<SyncPackageTrackingInfo> PackagesTrackingInfo { get; set; }

        public virtual TabletSyncLog TabletSyncLog { get; set; }
    }


    public class SyncPackageTrackingInfo
    {
        public virtual int Id { get; set; }
        public virtual string PackageId { get; set; }
        public virtual DateTime? PackageSyncTime { get; set; }
        public virtual string PackageType { get; set; }
        public virtual TabletSyncLogByUser TabletSyncLogByUser { get; set; }
        public virtual bool ReceivedByClient { get; set; }
    }
}
