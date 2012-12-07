using System;
using System.Collections.Generic;
using System.Text;
using Common.Utils;
using Synchronization.Core.Registration;
using Synchronization.Core.Interface;
using Synchronization.Core.Errors;

namespace Browsing.Supervisor.Registration
{
    public class SupervisorRegistrationManager : RegistrationManager
    {
        public SupervisorRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : base("CAPIRegistration.register", "SupervisorRegistration.register", requestProcessor, urlUtils, usbProvider)
        {
        }

        #region Override Methods

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

        protected override void OnStartRegistration(IServiceAuthorizationPacket packet)
        {
            System.Diagnostics.Debug.Assert(packet.Type == ServicePackectType.Request);

            AuthorizeAcceptedData(packet);

            base.OnStartRegistration(packet);
        }

        #endregion

        protected override void OnNewAuthorizationPacketsAvailable(IList<IServiceAuthorizationPacket> packets)
        {
            // uncomment to have automatic registration
            // DoRegistration(true);
        }

        protected override IList<IServiceAuthorizationPacket> OnReadUsbPackets(bool authorizationRequest)
        {
            return base.OnReadUsbPackets(true);
        }

        protected override void OnCheckPrerequisites(bool firstPhase)
        {
        }

        protected override IList<IServiceAuthorizationPacket> OnPrepareAuthorizationPackets(bool firstPhase, IList<IServiceAuthorizationPacket> webServicePackets)
        {
            return webServicePackets;
        }
    }
}
