using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Utils;
using Synchronization.Core.Registration;
using Synchronization.Core.Interface;
using Synchronization.Core.Errors;

namespace Browsing.Supervisor.Registration
{
    public class SupervisorRegistrationManager : RegistrationManager
    {
        public SupervisorRegistrationManager(IRequestProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : base("CAPIRegistration.register", "SupervisorRegistration.register", requestProcessor, urlUtils, usbProvider)
        {
        }

        #region Override Methods

        protected override Authorization DoInstantiateAuthService(IUrlUtils urlUtils, IRequestProcessor requestProcessor)
        {
            return new SupervisorAuthorization(urlUtils);
        }

        protected override string ContainerName
        {
            get { return CurrentUser.ToString(); } // bind to supervisor id
        }

        protected override Guid OnAcceptRegistrationId()
        {
            return CurrentUser; // bind to supervisr id
        }

        protected override string OnAcceptRegistrationName()
        {
            return string.Format("supervisor #'{0}'", RegistrationId); // todo: replace with true name
        }

        protected override void OnStartRegistration(IAuthorizationPacket packet)
        {
            System.Diagnostics.Debug.Assert(packet.PacketType == ServicePacketType.Request);

            // assign supervisor's id as registrar
            packet.SetRegistrator(CurrentUser);

            AuthorizeAcceptedData(packet);

            // prepare responce
            var responce = InstantiatePacket(false, packet.Channel);

            // keep tablet id as registration id
            responce.Data.RegistrationId = packet.Data.RegistrationId;

            if (responce.Channel != ServicePacketChannel.Usb)
                SendAuthorizationData(responce);

            base.OnStartRegistration(responce);

            packet.IsAuthorized = true;
        }

        #endregion

        protected override void OnAuthorizationPacketsAvailable(IList<IAuthorizationPacket> packets)
        {
            // uncomment to have automatic registration
            // DoRegistration(true);
        }

        protected override IList<IAuthorizationPacket> OnReadUsbPackets(bool authorizationRequest)
        {
            return base.OnReadUsbPackets(true);
        }

        protected override void OnCheckPrerequisites(bool firstPhase)
        {
        }

        protected override IList<IAuthorizationPacket> OnPrepareAuthorizationPackets(bool firstPhase, IList<IAuthorizationPacket> webServicePackets)
        {
            IList<IAuthorizationPacket> packets = webServicePackets.Where(p => !p.IsAuthorized && p.IsMarkedToAuthorize).ToList();
            if (packets.Count == 0)
                throw new RegistrationException("There are no new authorization requests", null);

            return packets;
        }
    }
}
