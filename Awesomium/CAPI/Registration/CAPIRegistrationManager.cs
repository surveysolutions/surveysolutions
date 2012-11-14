using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.Registration;
using Synchronization.Core.Errors;

namespace Browsing.CAPI.Registration
{
    public class CapiRegistrationManager : RegistrationManager
    {
        private readonly Guid DefaultDevice = new Guid("00000000-0000-0000-0000-111111111111");

        public CapiRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : base("SupervisorRegistration.register", "CAPIRegistration.register", requestProcessor, urlUtils, usbProvider)
        {
        }

        #region Override Methods

        protected override string ContainerName
        {
            get
            {
                return RegistrationId.ToString();
            }
        }

        protected override Guid OnAcceptRegistrationId()
        {
            return GetGuidFromProcessorId(); // bind to processor id
        }

        protected override string OnAcceptRegistrationName()
        {
            return Environment.MachineName;
        }

        protected override RegisterData OnFinalizeRegistration(string folderPath)
        {
            return AuthorizeAccepetedData(folderPath);
        }

        #endregion

        #region Helpers

        private Guid GetGuidFromProcessorId()
        {
            var cpuInfo = string.Empty;

            var management = new ManagementClass("win32_processor");
            var managementObjects = management.GetInstances();

            foreach (var mo in managementObjects)
            {
                cpuInfo = mo.Properties["processorID"].Value.ToString();
                break;
            }

            if (cpuInfo == null)
                return DefaultDevice;

            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(cpuInfo));
                var result = new Guid(hash);

                return result;
            }
        }


        #endregion
    }
}
