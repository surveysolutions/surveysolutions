using System.Collections.Generic;
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
            bundles.Add(new ScriptBundle("~/js/main").Include(new[]
            {
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/moment.js",
                "~/Scripts/modernizr-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/director.js",
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/knockout.mapping-latest.js",
                "~/Scripts/query-string.js",
                "~/Scripts/supervisor.framework.js",
                "~/Scripts/viewmodels/viewmodelbase.js",
                "~/Scripts/viewmodels/pagebase.js"
            }));

            bundles.Add(new ScriptBundle("~/validate").Include(new[]
            {
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive-custom-for-bootstrap.js"
            }));

            bundles.Add(new ScriptBundle("~/js/list").Include(new[]
            {
                "~/Scripts/ko.pager.js",
                "~/Scripts/viewmodels/listview.js"
            }));

            bundles.Add(new ScriptBundle("~/js/interview-general").Include(new[]
            {
                "~/Scripts/knockout.validation.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/Math.uuid.js",
                "~/Scripts/viewmodels/pages/interview/custom.js",
                "~/Scripts/lodash.underscore.js",
            }));

            bundles.Add(new ScriptBundle("~/js/interview-new").Include(new[]
            {
                "~/Scripts/viewmodels/pages/interview/new/datacontext.js",
                "~/Scripts/viewmodels/pages/interview/new/mapper.js",
                "~/Scripts/viewmodels/pages/interview/new/model.js",
                "~/Scripts/viewmodels/pages/interview/new/newinterview.js"
            }));

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