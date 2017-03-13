using System.Web.Configuration;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Code
{
    public static class ApplicationSettings
    {
        public static int InterviewDetailsDataSchedulerSynchronizationInterval => 
            WebConfigurationManager.AppSettings.GetInt("InterviewDetailsDataScheduler.SynchronizationInterval", 10);

        public static bool WebInterviewEnabled => 
            WebConfigurationManager.AppSettings.GetBool("WebInterviewEnabled", false);

        public static CustomErrorsMode CustomErrorsMode
        {
            get
            {
                CustomErrorsSection customErrors =
                    (CustomErrorsSection) WebConfigurationManager.GetSection("system.web/customErrors");
                return customErrors.Mode;
            }
        }
    }
}