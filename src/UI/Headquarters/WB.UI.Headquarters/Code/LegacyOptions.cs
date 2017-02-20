using System.Web.Configuration;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Code
{
    public static class LegacyOptions
    {
        public static int InterviewDetailsDataSchedulerSynchronizationInterval => 
            WebConfigurationManager.AppSettings.GetInt("InterviewDetailsDataScheduler.SynchronizationInterval", 10);

        public static bool WebInterviewEnabled => 
            WebConfigurationManager.AppSettings.GetBool("WebInterviewEnabled", false);
    }
}