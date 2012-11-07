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

        protected override bool OnStartRegistration(string folderPath, out RegisterData registeredData)
        {
            var data = GetFromRegistrationFile(folderPath + InFile);

            var deviceRegisterData = DeserializeRegisterData(Encoding.ASCII.GetString(data));

            var response = SendRegistrationRequest(data);
            var result = Encoding.UTF8.GetString(response, 0, response.Length);

            try
            {
                return string.Compare(result, "True", true) == 0 && base.OnStartRegistration(folderPath, out registeredData);
            }
            finally
            {
                registeredData = deviceRegisterData;
            }
        }

        protected override bool OnFinalizeRegistration(string folderPath, out RegisterData regData)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
