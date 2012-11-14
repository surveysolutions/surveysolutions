using System;
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
            get { return CurrentUser.ToString(); } // bind to supervisr id
        }

        protected override Guid OnAcceptRegistrationId()
        {
            return CurrentUser; // bind to supervisr id
        }

        protected override string OnAcceptRegistrationName()
        {
            return string.Format("supervisor #'{0}'", RegistrationId); // todo: replace with true name
        }

        protected override RegisterData OnStartRegistration(string folderPath)
        {
            return AuthorizeAccepetedData(folderPath);
        }

        #endregion
    }
}
