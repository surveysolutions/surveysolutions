using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.Documents
{
    public class TabletDocument : IView
    {
        public TabletDocument()
        {
            SyncLog = new Dictionary<Guid, List<TabletSyncLog>>();
        }

        public Guid DeviceId { get; set; }

        public string AndroidId { get; set; }

        public DateTime RegistrationDate { get; set; }

        public DateTime LastUpdateDate { get; set; }

        public List<Guid> Users { get; set; }

        public Dictionary<Guid, List<TabletSyncLog>> SyncLog { get; set; }
    }

    public class TabletSyncLog
    {
        public TabletSyncLog()
        {
            PackagesTrackingInfo = new Dictionary<string, PackagesTrackingInfo>
                                   {
                                       { SyncItemType.User, new PackagesTrackingInfo() },
                                       { SyncItemType.Questionnaire, new PackagesTrackingInfo() },
                                       { SyncItemType.Interview, new PackagesTrackingInfo() }
                                   };
        }

        public DateTime HandshakeInitTime { get; set; }

        public string AppVersion { get; set; }

        public Dictionary<string, PackagesTrackingInfo> PackagesTrackingInfo { get; set; }
    }

    public class PackagesTrackingInfo
    {
        public PackagesTrackingInfo()
        {
            PackagesRequestInfo = new Dictionary<string, DateTime?>();
        }

        public string LastPackageId { get; set; }

        public Dictionary<string, DateTime?> PackagesRequestInfo { get; set; }
    }
}
