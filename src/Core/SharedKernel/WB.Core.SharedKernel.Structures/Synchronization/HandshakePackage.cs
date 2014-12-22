﻿using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class HandshakePackage
    {
        public Guid SyncProcessKey;
        public Guid ClientInstanceKey;
        public Guid ClientRegistrationKey;
        
        
        public HandshakePackage()
        {
        }

        public HandshakePackage(Guid clientInstanceKey, Guid syncProcessKey, Guid clientRegistrationKey)
        {
            this.SyncProcessKey = syncProcessKey;
            this.ClientInstanceKey = clientInstanceKey;
            this.ClientRegistrationKey = clientRegistrationKey;
        }
    }
}
 