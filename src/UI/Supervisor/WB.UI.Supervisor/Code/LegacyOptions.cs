using System.Web.Configuration;

namespace WB.UI.Supervisor.Code
{
    public static class LegacyOptions
    {
        public static bool HqFunctionsEnabled
        {
            get { return bool.Parse(WebConfigurationManager.AppSettings["HeadquartersFunctionsEnabled"]); }
        }
    }
}