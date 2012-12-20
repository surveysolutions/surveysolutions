using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Synchronization.Core.Interface;

namespace Synchronization.Core.Errors
{
    public class RegistrationException : ServiceException
    {
        public RegistrationException(string message)
            : base(message, null)
        {
        }

        public RegistrationException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }

    public class RegistrationFaultException : RegistrationException
    {
        public IList<IAuthorizationPacket> FaultedPackets { get; private set; }

        public RegistrationFaultException(IAuthorizationPacket packet)
            : this(packet, null)
        {
        }

        public RegistrationFaultException(IAuthorizationPacket packet, Exception ex)
            : this(new List<IAuthorizationPacket>() { packet }, ex)
        {
        }

        public RegistrationFaultException(IList<IAuthorizationPacket> packets)
            : this(packets, null)
        {
            FaultedPackets = packets;
        }

        public RegistrationFaultException(IList<IAuthorizationPacket> packets, Exception ex)
            : base("Registration failed", ex)
        {
            FaultedPackets = packets;
        }
    }
}
