// -----------------------------------------------------------------------
// <copyright file="Authorization.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Synchronization.Core.Interface;
using Synchronization.Core.Registration;
using Common.Utils;

namespace Browsing.CAPI.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class is reponsible for watching authorization packets authorized by supervisor and available via net.
    /// This class doesn't access supervisor's WCF service  directly, but via CAPI-web
    /// </summary>
    public class CAPIAuthorization : Authorization
    {
        #region Members

        IRequestProcessor requestProcessor;
        IUrlUtils urlUtils;
        Guid tabletId;

        #endregion

        #region C-tor

        public CAPIAuthorization(IUrlUtils urlUtils, IRequestProcessor requestProcessor, Guid tabletId)
            : base()
        {
            this.requestProcessor = requestProcessor;
            this.urlUtils = urlUtils;
            this.tabletId = tabletId;
        }

        #endregion

        #region Oveloaded

        protected override IList<IAuthorizationPacket> OnCollectAuthorizationPackets(IList<IAuthorizationPacket> availablePackets)
        {
            var url = urlUtils.GetCheckConfirmedAuthorizationUrl(this.tabletId);
            var regData = this.requestProcessor.Process<string>(url, "False");

            if (string.Compare(regData, "False", true) != 0)
            {
                try
                {
                    var content = RegistrationManager.DeserializeContent<List<RegisterData>>(regData);
                    if (content != null)
                    {
                        var lastData = content.OrderBy(d => d.RegisterDate).LastOrDefault();
                        if (lastData != null)
                        {
                            var packet = new AuthorizationPacket(lastData, ServicePacketChannel.Net, ServicePacketType.Responce);
                            packet.IsAuthorized = true;

                            availablePackets.Add(packet);
                        }
                    }
                }
                catch
                {
                }
            }

            return availablePackets;
        }

        #endregion
    }
}
