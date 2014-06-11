using System.Web.Mvc;
using System.Web.Optimization;

namespace WB.Core.SharedKernels.SurveyManagement.Web
{
    public class SurveyManagementAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "SurveyManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            RegisterBundles(BundleTable.Bundles);
            RegisterRoutes(context);
        }

        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/js/interview-details").Include(new[]
            {
                "~/Scripts/director.js",
                "~/Scripts/viewmodels/pages/interview/details/config.js",
                "~/Scripts/viewmodels/pages/interview/details/datacontext.js",
                "~/Scripts/viewmodels/pages/interview/details/interviewdetailssettings.js",
                "~/Scripts/viewmodels/pages/interview/details/interviewdetails.js",
            }));
        }

        private static void RegisterRoutes(AreaRegistrationContext context)
        {
            context.MapRoute(
                "SurveyManagement_default",
                "SurveyManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "Interview",
                "Interview/{action}/{id}",
                new { controller = "Interview", id = UrlParameter.Optional });
        }
    }
}