using System;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Main.Core.Documents
{
    public class ClientDeviceDocument : IView
    {
        public Guid Id { set; get; }

        public string DeviceId { set; get; }

        public DateTime CreatedDate { set; get; }

        public DateTime ModificationDate { set; get; }

        public string DeviceType { set; get; }

        public Guid ClientInstanceKey { set; get; }

        public long LastSyncItemIdentifier { set; get; }
        
    }
}
