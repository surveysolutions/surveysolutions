using System;
using System.Collections.Generic;

using Main.Core.Entities.SubEntities;

using WB.Core.Synchronization.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation
{
    public class TabletLogView
    {
        public TabletLogView()
        {
            SyncLog = new List<UserSyncLogView>();
        }
        public Guid DeviceId { get; set; }

        public string AndroidId { get; set; }

        public DateTime RegistrationDate { get; set; }

        public DateTime LastUpdateDate { get; set; }

        public List<UserLight> Users { get; set; }

        public List<UserSyncLogView> SyncLog { get; set; }
    }

    public class UserSyncLogView
    {
        public UserSyncLogView()
        {
            this.TabletSyncLog = new List<TabletSyncLogView>();
        }
        public UserLight User { get; set; }

        public List<TabletSyncLogView> TabletSyncLog { get; set; }
    }

    public class TabletSyncLogView
    {
        public string AppVersion { get; set; }

        public DateTime HandshakeTime { get; set; }

        public Dictionary<string, PackagesTrackingInfo> PackagesTrackingInfo { get; set; }
    }
}