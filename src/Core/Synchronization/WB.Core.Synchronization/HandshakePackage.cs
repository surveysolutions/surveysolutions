namespace WB.Core.Synchronization
{
    using System;

    public class HandshakePackage
    {
        public Guid SyncProcessKey;
        public Guid ClientInstanceKey;

        public HandshakePackage(Guid clientInstanceKey)
        {
            this.SyncProcessKey = Guid.NewGuid();
            this.ClientInstanceKey = clientInstanceKey;
        }
    }
}
 