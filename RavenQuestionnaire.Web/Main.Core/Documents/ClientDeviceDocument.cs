using System;

namespace Main.Core.Documents
{
    public class ClientDeviceDocument
    {
        public Guid Id;

        public string DeviceId;

        public DateTime CreatedDate;

        public DateTime ModificationDate;

        public string DeviceType;

        public Guid ClientInstanceKey;
    }
}
