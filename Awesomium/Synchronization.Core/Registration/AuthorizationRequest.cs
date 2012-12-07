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
        public AuthorizationRequest(RegisterData data, bool viaUsbChannel)
            : base(data, viaUsbChannel)
        {
        }

        public override Interface.ServicePackectType Type { get { return Interface.ServicePackectType.Request; } }
    }
}
