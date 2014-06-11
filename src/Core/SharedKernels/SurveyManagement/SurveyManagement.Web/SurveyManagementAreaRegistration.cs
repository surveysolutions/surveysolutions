using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;

namespace WB.Core.SharedKernels.SurveyManagement.Web
{
    public class SurveyManagementAreaRegistration : AreaRegistration 
    {
        private static readonly Dictionary<string, string[]> ScriptBundles = new Dictionary<string, string[]>
        {
            {
                "~/js/main", new[]
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
                    "~/Scripts/viewmodels/pagebase.js",
                }
            },
            {
                "~/validate", new[]
                {
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.validate.unobtrusive-custom-for-bootstrap.js"
                }
            },
            {
                "~/js/list", new[]
                {
                    "~/Scripts/ko.pager.js",
                    "~/Scripts/viewmodels/listview.js"
                }
            },
            {
                "~/js/interview-general", new[]
                {
                    "~/Scripts/knockout.validation.js",
                    "~/Scripts/bootstrap-datepicker.js",
                    "~/Scripts/Math.uuid.js",
                    "~/Scripts/viewmodels/pages/interview/custom.js",
                    "~/Scripts/lodash.underscore.js",
                }
            },
            {
                "~/js/interview-new", new[]
                {
                    "~/Scripts/viewmodels/pages/interview/new/datacontext.js",
                    "~/Scripts/viewmodels/pages/interview/new/mapper.js",
                    "~/Scripts/viewmodels/pages/interview/new/model.js",
                    "~/Scripts/viewmodels/pages/interview/new/newinterview.js"
                }
            },
            {
                "~/js/interview-details", new[]
                {
                    "~/Scripts/director.js",
                    "~/Scripts/viewmodels/pages/interview/details/config.js",
                    "~/Scripts/viewmodels/pages/interview/details/datacontext.js",
                    "~/Scripts/viewmodels/pages/interview/details/interviewdetailssettings.js",
                    "~/Scripts/viewmodels/pages/interview/details/interviewdetails.js",
                }
            },
        };

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
            foreach (KeyValuePair<string, string[]> scriptBundle in ScriptBundles)
            {
                bundles.Add(new ScriptBundle(scriptBundle.Key).Include(scriptBundle.Value));
            }
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