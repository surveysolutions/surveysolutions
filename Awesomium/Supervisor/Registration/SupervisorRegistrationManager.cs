using System;
using System.Text;
using Common.Utils;
using Synchronization.Core.Registration;
using Synchronization.Core.Interface;

namespace Browsing.Supervisor.Registration
{
    public class SupervisorRegistrationManager : RegistrationManager
    {
        public SupervisorRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : base("CAPIRegistration.register", "SupervisorRegistration.register", requestProcessor, urlUtils, usbProvider)
        {
        }


        #region Override Methods

        protected override bool OnStartRegistration(string folderPath)
        {
            var data = GetFromRegistrationFile(folderPath + InFile);
            var response = SendRegistrationRequest(data);
            var result = Encoding.UTF8.GetString(response, 0, response.Length);

            return string.Compare(result, "True", true) == 0 && base.OnStartRegistration(folderPath);
        }

        protected override bool OnFinalizeRegistration(string folderPath)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
