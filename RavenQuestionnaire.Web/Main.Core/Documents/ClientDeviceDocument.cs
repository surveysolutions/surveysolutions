using System;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Main.Core.Documents
{
    public class ClientDeviceDocument : IView
    {
        public Guid Id;

        public string DeviceId;

        public DateTime CreatedDate;

        public DateTime ModificationDate;

        public string DeviceType;

        public Guid ClientInstanceKey;
    }
}
