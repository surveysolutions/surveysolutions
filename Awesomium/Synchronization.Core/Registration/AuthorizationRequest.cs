// -----------------------------------------------------------------------
// <copyright file="AuthorizationRequest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Synchronization.Core.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Synchronization.Core.Interface;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class AuthorizationRequest : AuthorizationPacket, IRequestPacket
    {
        public AuthorizationRequest(RegisterData data, ServicePacketChannel channel)
            : base(data, channel)
        {
        }

        public override Interface.ServicePacketType Type { get { return Interface.ServicePacketType.Request; } }
    }
}
