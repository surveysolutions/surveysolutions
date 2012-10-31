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
        #region Private fields

        private readonly Guid tabletId = Guid.Parse("20000000-0000-0000-0000-000000000000");

        #endregion

        #region Override Methods

        public override void RegistrationFirstStep(IRSACryptoService rsaCryptoService, string user, string url)
        {
            var data = GetFromRegistrationFile("G:/CAPIRegistration.register");
            var response = SendPostWebRequest(url, data);
            var result = Encoding.UTF8.GetString(response, 0, response.Length);

            if (result == "True")
            {
                var dataToFile = Encoding.ASCII.GetBytes(SerializeRegisterData(new RegisterData { SecretKey = rsaCryptoService.GetPublicKey(user).Modulus, TabletId = this.tabletId }));

                FormRegistrationFile(dataToFile, "G:/SupervisorRegistration.register");
            }

        }

        public override bool RegistrationSecondStep(IRSACryptoService rsaCryptoService, string url)
        {
            throw new NotImplementedException();
        }

        public override void RegistrationFirstStep(IRSACryptoService rsaCryptoService)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
