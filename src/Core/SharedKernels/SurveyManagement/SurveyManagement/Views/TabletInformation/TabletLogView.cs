using System;
using System.Collections.Generic;

using WB.Core.Synchronization.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation
{
    public class TabletLogView
    {
        public Guid DeviceId { get; set; }

        public string AndroidId { get; set; }

        public DateTime RegistrationDate { get; set; }

        public DateTime LastUpdateDate { get; set; }

        public List<Guid> Users { get; set; }

        public Dictionary<Guid, List<TabletSyncLog>> SyncLog { get; set; }
    }
}