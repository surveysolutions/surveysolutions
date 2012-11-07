using System;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Common.Utils;
using Synchronization.Core.Registration;
using Synchronization.Core.Interface;

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
                return RegisrationId.ToString();
            }
        }

        protected override Guid OnAcceptRegistrationId()
        {
            return GetGuidFromProcessorId();
        }

        protected override string OnAcceptRegistrationName()
        {
            return Environment.MachineName;
        }

        protected override bool OnFinalizeRegistration(string folderPath, out RegisterData registeredData)
        {
            var data = GetFromRegistrationFile(folderPath + InFile);

            var supervisorRegisterData = DeserializeRegisterData(Encoding.ASCII.GetString(data));

            var response = SendRegistrationRequest(data);
            var result = Encoding.UTF8.GetString(response, 0, response.Length);

            try
            {
                return string.Compare(result, "True", true) == 0;
            }
            finally
            {
                registeredData = supervisorRegisterData;
            }
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
