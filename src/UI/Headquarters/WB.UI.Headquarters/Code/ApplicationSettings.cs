using System.Web.Configuration;
using NConfig;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Code
{
    public static class ApplicationSettings
    {
        public static int InterviewDetailsDataSchedulerSynchronizationInterval => 
            WebConfigurationManager.AppSettings.GetInt("InterviewDetailsDataScheduler.SynchronizationInterval", 10);

        public static bool WebInterviewEnabled => 
            WebConfigurationManager.AppSettings.GetBool("WebInterviewEnabled", false);

        public static bool NewVersionCheckEnabled =>
            WebConfigurationManager.AppSettings.GetBool("NewVersionCheckEnabled", false);

        public static string NewVersionCheckUrl =>
            WebConfigurationManager.AppSettings.GetString("NewVersionCheckUrl", string.Empty);

        public static CustomErrorsMode CustomErrorsMode
        {
            get
            {
                CustomErrorsSection customErrors = NConfigurator.Default.GetSection<CustomErrorsSection>(@"system.web/customErrors");
                return customErrors.Mode;
            }
        }
    }
}
