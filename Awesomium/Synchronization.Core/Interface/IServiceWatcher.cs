// -----------------------------------------------------------------------
// <copyright file="IServiceWatcher.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Synchronization.Core.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public delegate void AuthorizationPacketsAlarm(IList<IServiceAuthorizationPacket> packets);

    /// <summary>
    /// Watches web part for specific service requests
    /// </summary>
    public interface IServiceWatcher
    {
        event AuthorizationPacketsAlarm NewPacketsAvailable;

        /// <summary>
        /// Collects packets available on web-part and unites them with additional packets passed through parameter
        /// </summary>
        void CollectAuthorizationPackets(IList<IServiceAuthorizationPacket> extraPackets);
    }
}
