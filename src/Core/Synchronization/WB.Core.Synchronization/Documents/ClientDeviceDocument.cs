using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.Documents
{
    public class ClientDeviceDocument : IView
    {
        public Guid PublicKey { set; get; }

        public string DeviceId { set; get; }

        public DateTime CreatedDate { set; get; }

        public DateTime ModificationDate { set; get; }

        public string DeviceType { set; get; }

        public Guid ClientInstanceKey { set; get; }

        public long LastSyncItemIdentifier { set; get; }

        public Guid SupervisorKey { set; get; }

    }
}
