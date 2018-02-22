using System.Configuration;
using WB.UI.Shared.Web.Extensions;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public sealed class AppSettings
    {
        public static readonly AppSettings Instance = new AppSettings();

        public static bool IsDebugBuilded
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public string GoogleMapApiKey
        {
            get { return ConfigurationManager.AppSettings.GetString("Google.Map.ApiKey"); }
        }

        public string GoogleMapApiBaseUrl
        {
            get { return ConfigurationManager.AppSettings.GetString("Google.Map.BaseApiUrl", "https://maps.googleapis.com"); }
        }

        public string GoogleMapBaseUrl
        {
            get { return ConfigurationManager.AppSettings.GetString("Google.Map.BaseUrl", "https://google.com"); }
        }

        public string AdminEmail
        {
            get { return ConfigurationManager.AppSettings.GetString("AdminEmail"); }
        }

        public bool AcceptUnsignedCertificate
        {
            get { return ConfigurationManager.AppSettings.GetBool("AcceptUnsignedCertificate", true); }
        }
    }
}