namespace WB.Core.Synchronization
{
    using System;

    public class HandshakePackage
    {
        public Guid SyncProcessKey;
        public Guid ClientInstanceKey;
        public Guid ClientRegistrationKey;


        public HandshakePackage(Guid clientInstanceKey, Guid syncProcessKey, Guid clientRegistrationKey)
        {
            this.SyncProcessKey = syncProcessKey;
            this.ClientInstanceKey = clientInstanceKey;
            this.ClientInstanceKey = clientRegistrationKey;
        }
    }
}
 