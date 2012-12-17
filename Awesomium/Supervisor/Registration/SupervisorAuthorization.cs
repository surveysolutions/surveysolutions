// -----------------------------------------------------------------------
// <copyright file="Authorization.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Synchronization.Core.Interface;
using Synchronization.Core.Registration;
using Common.Utils;

namespace Browsing.Supervisor.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class is reponsible for watching authorization packets send by CAPI via net.
    /// This class accesses supervisor's WCF service directly
    /// </summary>
    public class SupervisorAuthorization : Authorization
    {
        #region Members

        private IUrlUtils urlUtils;
        private AuthorizationServiceClient authorizationClient;

        #endregion

        #region C-tor

        public SupervisorAuthorization(IUrlUtils urlUtils)
            : base()
        {
            this.urlUtils = urlUtils;
        }

        #endregion

        #region Oveloaded

        protected override IList<IAuthorizationPacket> OnCollectAuthorizationPackets(IList<IAuthorizationPacket> availablePackets)
        {
            if (this.authorizationClient == null) // postponed to instantiate to make sure about correct AuthServiceUrl
                this.authorizationClient = new AuthorizationServiceClient(this.urlUtils.GetAuthServiceUrl());

            AuthPackets packets = this.authorizationClient.PickupAuthorizationPackets(Guid.Empty);

            var nonauthorizedPacketsList = new List<IAuthorizationPacket>();

            foreach (var p in packets.Packets)
            {
                var pkt = p as IAuthorizationPacket;
                if (pkt == null || pkt.IsAuthorized)
                    continue;

                // fix channel
                pkt.SetChannel(ServicePacketChannel.Net);
                nonauthorizedPacketsList.Add(pkt);
            }

            return nonauthorizedPacketsList.Union(availablePackets, this).ToList();
        }

        #endregion
    }
}
