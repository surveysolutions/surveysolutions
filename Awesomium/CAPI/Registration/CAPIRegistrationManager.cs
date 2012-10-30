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
        private readonly Guid tabletId = Guid.Parse("10000000-0000-0000-0000-000000000000");


        public override void RegistrationFirstStep(IRSACryptoService rsaCryptoService)
        {
            var a = rsaCryptoService.GetPublicKey(this.tabletId.ToString()).Modulus;
            var dataToFile = Encoding.ASCII.GetBytes(SerializeRegisterData(new RegisterData { SecretKey = a, TabletId = this.tabletId }));

            FormRegistrationFile(dataToFile, "G:/CAPIRegistration.register");
        }

        public override void RegistrationFirstStep(IRSACryptoService rsaCryptoService, string user)
        {
            throw new NotImplementedException();
        }

        public override void RegistrationSecondStep()
        {
            //var resp = SendPostWebRequest("tabletId="+this.tabletId);

            //var tempString = Encoding.UTF8.GetString(resp, 0, resp.Length);

            //var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };

            //var data = JsonConvert.DeserializeObject<RegisterData>(tempString, settings);

            
        }
    }
}
