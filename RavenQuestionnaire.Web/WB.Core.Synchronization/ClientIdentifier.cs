namespace WB.Core.Synchronization
{
    using System;

    public class ClientIdentifier
    {
        public Guid ClientInstanceKey;
        public Guid? ClientKey;

        public string ClientDeviceKey;
        public string ClientVersionIdentifier;
        
        public Guid? LastSyncKey;

        public Guid? CurrentProcessKey;

        public ClientIdentifier()
        {

        }
    }
}
