using System;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Designer.Implementation.Services
{
    public class RecaptchaService : IRecaptchaService
    {
        private const string RECAPTHAAPIURL = "https://www.google.com/recaptcha/api/siteverify";
        private readonly Core.GenericSubdomains.Portable.Services.ILogger logger;
        private readonly IConfigurationManager configurationManager;

        public RecaptchaService(Core.GenericSubdomains.Portable.Services.ILogger logger, IConfigurationManager configurationManager)
        {
            this.logger = logger;
            this.configurationManager = configurationManager;
        }

        private class RecaptchaResponse
        {
            public bool success { get; set; }
            public string errorcodes { get; set; }
        }

        public bool IsValid(string clientResponse)
        {
            string secretKey = this.configurationManager.AppSettings["reCaptchaPrivateKey"];

            RecaptchaResponse recaptchaResponse = null;

            try
            {
                using (var webClient = new WebClient())
                {
                    string userHostAddress = HttpContext.Current.Request.IsLocal
                        ? "localhost"
                        : HttpContext.Current.Request.UserHostAddress;

                    string webResponseFromRecaptcha = webClient.DownloadString(
                            $"{RECAPTHAAPIURL}?secret={secretKey}&response={clientResponse}&remoteip={userHostAddress}");

                    recaptchaResponse = new JavaScriptSerializer().Deserialize<RecaptchaResponse>(webResponseFromRecaptcha);
                }
            }
            catch (Exception ex)
            {
                this.logger.Warn("Couldn't get recaptcha response from goolge.", ex);
            }
            
            return recaptchaResponse == null || recaptchaResponse.success;
        }
    }
}