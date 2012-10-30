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
        public override void RegistrationFirstStep(IRSACryptoService rsaCryptoService)
        {
            throw new NotImplementedException();
        }

        public override void RegistrationFirstStep(IRSACryptoService rsaCryptoService, string user)
        {
            var data = GetFromRegistrationFile("G:/CAPIRegistration.register");

            var response = SendPostWebRequest("http://localhost:8084/EmportExport/StartRegister", "registerFile="+Encoding.UTF8.GetString(data, 0, data.Length));

            var tempString = Encoding.UTF8.GetString(response, 0, response.Length);

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };

            var registerdata = JsonConvert.DeserializeObject<RegisterData>(tempString, settings);
        }

        public override void RegistrationSecondStep()
        {
            throw new NotImplementedException();
        }
    }
}
