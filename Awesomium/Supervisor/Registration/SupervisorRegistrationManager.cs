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

        protected override RegisterData OnStartRegistration(string folderPath)
        {
            return AuthorizeAccepetedData(folderPath);
        }

        #endregion
    }
}
