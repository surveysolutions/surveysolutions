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
        #region Private fields

        private readonly Guid tabletId = Guid.Parse("10000000-0000-0000-0000-000000000000");

        #endregion

        #region Override Methods

        public override void RegistrationFirstStep(IRSACryptoService rsaCryptoService)
        {
            var publicKey = rsaCryptoService.GetPublicKey(this.tabletId.ToString()).Modulus;
            var dataToFile = Encoding.ASCII.GetBytes(SerializeRegisterData(new RegisterData { SecretKey = publicKey, TabletId = this.tabletId }));

            FormRegistrationFile(dataToFile, "G:/CAPIRegistration.register");
        }

        public override bool RegistrationSecondStep(IRSACryptoService rsaCryptoService, string url)
        {
            var data = GetFromRegistrationFile("G:/SupervisorRegistration.register");
            var response = SendPostWebRequest(url, data);
            var result = Encoding.UTF8.GetString(response, 0, response.Length);

            if (result == "True")
            {
                return true;
            }
            return false;
        }

        public override void RegistrationFirstStep(IRSACryptoService rsaCryptoService, string user, string url)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
