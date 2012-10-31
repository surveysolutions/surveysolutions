using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Synchronization.Core.Registration;

namespace Browsing.Supervisor.Registration
{
    public class SupervisorRegistrationManager : RegistrationManager
    {
        public SupervisorRegistrationManager()
            : base("CAPIRegistration.register", "SupervisorRegistration.register")
        {
        }


        #region Override Methods

        protected override Guid OnAcceptId()
        {
            return new Guid("{20000000-0000-0000-0000-000000000000}");
        }

        public override bool StartRegistration(string folderPath, string keyContainerName, string url)
        {
            var data = GetFromRegistrationFile(folderPath + InFile);
            var response = SendRegistrationRequest(url, data);
            var result = Encoding.UTF8.GetString(response, 0, response.Length);

            // G:/SupervisorRegistration.register"
            return string.Compare(result, "True", true) == 0 && base.StartRegistration(folderPath, keyContainerName, url);
        }

        public override bool FinalizeRegistration(string folderPath, string url)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
