using System;
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
            return new Guid("{10000000-0000-0000-0000-000000000000}");
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
            catch(Exception e)
            {
                throw e;
            }
        }

        #endregion
    }
}
