using System;
using System.Text;
using Common.Utils;
using Synchronization.Core.Registration;

namespace Browsing.Supervisor.Registration
{
    public class SupervisorRegistrationManager : RegistrationManager
    {
        public SupervisorRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils)
            : base("CAPIRegistration.register", "SupervisorRegistration.register", requestProcessor, urlUtils)
        {
        }


        #region Override Methods

        public override bool StartRegistration(string folderPath)
        {
            try
            {
                var data = GetFromRegistrationFile(folderPath + InFile);
                var response = SendRegistrationRequest(data);
                var result = Encoding.UTF8.GetString(response, 0, response.Length);

                return string.Compare(result, "True", true) == 0 && base.StartRegistration(folderPath);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public override bool FinalizeRegistration(string folderPath)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
