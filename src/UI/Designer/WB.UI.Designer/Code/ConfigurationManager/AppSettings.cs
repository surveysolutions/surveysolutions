using System;
using System.Configuration;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Designer
{
    public sealed class AppSettings
    {
        public static bool IsDebugRelease
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

        public static readonly AppSettings Instance = new AppSettings();

        public bool IsReCaptchaEnabled => ConfigurationManager.AppSettings.GetBool("IsReCaptchaEnabled", true);

        public string WKHtmlToPdfExecutablePath => ConfigurationManager.AppSettings.GetString("WKHtmlToPdfExecutablePath");

        public bool IsTrackingEnabled => ConfigurationManager.AppSettings.GetBool("IsTrackingEnabled", false);

        public int StorageLoadingChunkSize => ConfigurationManager.AppSettings.GetInt("StorageLoadingChunkSize", 1024);

        public string SupportEmail => ConfigurationManager.AppSettings.GetString("SupportEmail");

        public bool IsApiSslVerificationEnabled => ConfigurationManager.AppSettings.GetBool("IsApiSSLVerificationEnabled", true);

        public int CountOfFailedLoginAttemptsBeforeCaptcha => ConfigurationManager.AppSettings.GetInt("CountOfFailedLoginAttemptsBeforeCaptcha", 5);

        public TimeSpan TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt => TimeSpan.FromMinutes(
            ConfigurationManager.AppSettings.GetInt("TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt", 5));
    }
}