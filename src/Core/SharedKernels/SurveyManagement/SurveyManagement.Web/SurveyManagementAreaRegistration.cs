using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;

namespace WB.Core.SharedKernels.SurveyManagement.Web
{
    public class SurveyManagementAreaRegistration : AreaRegistration
    {
        private static readonly Dictionary<string, string[]> StyleBundles = new Dictionary<string, string[]>
        {
            {
                "~/Content/main", new[]
                {
                    "~/Content/bootstrap.css",
                    "~/Content/font-awesome.min.css",
                    "~/Content/bootstrap-mvc-validation.css",
                    "~/Content/jquery.pnotify.default.css",
                    "~/Content/app.css",
                }
            },
            {
                "~/css/main-not-loggedin", new[]
                {
                    "~/Content/bootstrap.css",
                    "~/Content/bootstrap-mvc-validation.css",
                    "~/Content/main-not-logged.css",
                }
            },
            {
                "~/css/list", new[]
                {
                    "~/Content/listview.css",
                }
            },
            {
                "~/css/interview-new", new[]
                {
                    "~/Content/bootstrap-editable.css",
                    "~/Content/datepicker.css",
                }
            },
            {
                "~/css/interview", new[]
                {
                    "~/Content/bootstrap-editable.css",
                    "~/Content/datepicker.css"
                }
            },

            {
                "~/css/admin", new[]
                {
                    "~/Content/bootstrap.css",
                    "~/Content/admin.css"
                }
            }
        };

        private static readonly Dictionary<string, string[]> ScriptBundles = new Dictionary<string, string[]>
        {
            {
                "~/js/main", new[]
                {
                    "~/Scripts/jquery-2.1.0.js",
                    "~/Scripts/moment.js",
                    "~/Scripts/modernizr-2.7.2.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/director.js",
                    "~/Scripts/knockout-3.1.0.js",
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

            foreach (KeyValuePair<string, string[]> styleBundle in StyleBundles)
            {
                bundles.Add(new StyleBundle(styleBundle.Key).Include(styleBundle.Value));
            }
        }

        private static void RegisterRoutes(AreaRegistrationContext context)
        {
            context.MapRoute(
                "InterviewRoute",
                "Interview/{action}/{id}",
                new { controller = "Interview", id = UrlParameter.Optional });

            context.MapRoute(
                "BackupRoute",
                "Backup/{action}/{id}",
                new { controller = "Backup", action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "HQRoute",
                "HQ/{action}/{id}",
                new { controller = "HQ", action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "ImportExportRoute",
                "ImportExport/{action}/{id}",
                new { controller = "ImportExport", action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "SurveyRoute",
                "Survey/{action}/{id}",
                new { controller = "Survey", action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "TabletReportRoute",
                "TabletReport/{action}/{id}",
                new { controller = "TabletReport", action = "Index", id = UrlParameter.Optional });
        }
    }
}