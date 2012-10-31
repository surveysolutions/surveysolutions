using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Browsing.CAPI.Synchronization;
using Newtonsoft.Json;
using Synchronization.Core.Registration;


namespace Browsing.CAPI.Registration
{
    public class CapiRegistrationManager : RegistrationManager
    {
        public CapiRegistrationManager()
            : base("SupervisorRegistration.register", "CAPIRegistration.register")
        {
        }

        #region Override Methods

        protected override Guid OnAcceptId()
        {
            return new Guid("{10000000-0000-0000-0000-000000000000}");
        }

        public override bool StartRegistration(string folderPath, string keyContainerName = null, string url = null)
        {
            return base.StartRegistration(folderPath, Id.ToString(), url);
        }

        public override bool FinalizeRegistration(string folderPath, string url)
        {
            var data = GetFromRegistrationFile(folderPath + InFile);

            var response = SendRegistrationRequest(url, data);
            var result = Encoding.UTF8.GetString(response, 0, response.Length);

            return string.Compare(result, "True", true) == 0;
        }

        #endregion
    }
}
