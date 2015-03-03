using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class HandshakePackage
    {
        public Guid SyncProcessKey;
        public Guid ClientInstanceKey;
        public Guid ClientRegistrationKey;
        public Guid UserId;
        
        public HandshakePackage()
        {
        }

        public HandshakePackage(Guid userId, Guid clientInstanceKey, Guid syncProcessKey, Guid clientRegistrationKey)
        {
            this.UserId = userId;
            this.SyncProcessKey = syncProcessKey;
            this.ClientInstanceKey = clientInstanceKey;
            this.ClientRegistrationKey = clientRegistrationKey;
        }
    }
}
 