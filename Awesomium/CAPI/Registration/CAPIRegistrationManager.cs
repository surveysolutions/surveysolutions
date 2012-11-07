using System;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Common.Utils;
using Synchronization.Core.Registration;


namespace Browsing.CAPI.Registration
{
    public class CapiRegistrationManager : RegistrationManager
    {
        public CapiRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils)
            : base("SupervisorRegistration.register", "CAPIRegistration.register", requestProcessor, urlUtils)
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

        public override bool FinalizeRegistration(string folderPath)
        {
            try
            {
                var data = GetFromRegistrationFile(folderPath + InFile);

                var response = SendRegistrationRequest(data);
                var result = Encoding.UTF8.GetString(response, 0, response.Length);

                return string.Compare(result, "True", true) == 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion
        #region Helpers

        private Guid GetGuidFromProcessorId()
        {
            var cpuInfo = string.Empty;

            var mc = new ManagementClass("win32_processor");
            var moc = mc.GetInstances();
            foreach (var mo in moc.Cast<ManagementBaseObject>().Where(mo => cpuInfo == ""))
            {
                cpuInfo = mo.Properties["processorID"].Value.ToString();
                break;
            }

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
