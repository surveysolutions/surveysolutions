using System.Web.Configuration;

namespace WB.UI.Supervisor.Code
{
    public static class LegacyOptions
    {
        public static bool SchedulerEnabled
        {
            get { return bool.Parse(WebConfigurationManager.AppSettings["Scheduler.Enabled"]); }
        }

        public static int InterviewDetailsDataSchedulerSynchronizationInterval
        {
            get
            {
                return
                    int.Parse(
                        WebConfigurationManager.AppSettings["InterviewDetailsDataScheduler.SynchronizationInterval"]);
            }
        }

        public static string SynchronizationIncomingCapiPackagesWithErrorsDirectory
        {
            get
            {
                return WebConfigurationManager.AppSettings["Synchronization.IncomingCapiPackagesWithErrorsDirectory"];
            }
        }

        public static string SynchronizationIncomingCapiPackageFileNameExtension
        {
            get
            {
                return WebConfigurationManager.AppSettings["Synchronization.IncomingCapiPackageFileNameExtension"];
            }
        }

        public static string IncomingUnprocessedPackageFileNameExtension
        {
            get
            {
                return WebConfigurationManager.AppSettings["Synchronization.IncomingUnprocessedPackageFileNameExtension"];
            }
        }
        public static int RetryCount
        {
            get
            {
                return
                    int.Parse(
                        WebConfigurationManager.AppSettings["InterviewDetailsDataScheduler.RetryCount"]);
            }
        }
    }
}