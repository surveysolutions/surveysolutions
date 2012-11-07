using System;
using System.Text;
using Common.Utils;
using Synchronization.Core.Registration;
using Synchronization.Core.Interface;

namespace Browsing.CAPI.Registration
{
    public class CapiRegistrationManager : RegistrationManager
    {
        public CapiRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : base("SupervisorRegistration.register", "CAPIRegistration.register", requestProcessor, urlUtils, usbProvider)
        {
        }

        #region Override Methods

        protected override string ContainerName
        {
            get
            {
                return RegisrationId.ToString();
            }
        }

        protected override Guid OnAcceptRegistrationId()
        {
            return new Guid("{10000000-0000-0000-0000-000000000000}");
        }

        protected override bool OnFinalizeRegistration(string folderPath)
        {
            var data = GetFromRegistrationFile(folderPath + InFile);

            var response = SendRegistrationRequest(data);
            var result = Encoding.UTF8.GetString(response, 0, response.Length);

            return string.Compare(result, "True", true) == 0;
        }

        #endregion
    }
}
