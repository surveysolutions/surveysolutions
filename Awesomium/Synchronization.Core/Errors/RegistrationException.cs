using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Synchronization.Core.Interface;

namespace Synchronization.Core.Errors
{
    public class RegistrationException : ServiceException
    {
        public RegistrationException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }

    public class RegistrationFaultException : RegistrationException
    {
        public IList<IServiceAuthorizationPacket> FaultedPackets { get; private set; }

        public RegistrationFaultException(IServiceAuthorizationPacket packet)
            : this(packet, null)
        {
        }

        public RegistrationFaultException(IServiceAuthorizationPacket packet, Exception ex)
            : this(new List<IServiceAuthorizationPacket>() { packet }, ex)
        {
        }

        public RegistrationFaultException(IList<IServiceAuthorizationPacket> packets)
            : this(packets, null)
        {
            FaultedPackets = packets;
        }

        public RegistrationFaultException(IList<IServiceAuthorizationPacket> packets, Exception ex)
            : base("Registration failed", ex)
        {
            FaultedPackets = packets;
        }
    }
}
