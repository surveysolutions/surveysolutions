using System.Web.Optimization;
using Raven.Client.Linq;

namespace WB.UI.Headquarters.App_Start
{
    public static class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*", "~/Scripts/ValidationFix.js"));
            bundles.Add(new ScriptBundle("~/bundles/main")
                .Include("~/Scripts/knockout-{version}.js",
                "~/Scripts/knockout.mapping-latest.js",
                "~/Scripts/supervisor.framework.js",
                "~/Scripts/query-string.js",
                "~/Scripts/modernizr-{version}.js",
                "~/Scripts/viewmodels/viewmodelbase.js",
                 "~/Scripts/viewmodels/pagebase.js"));


            bundles.Add(new ScriptBundle("~/js/list").Include(
                        "~/Scripts/ko.pager.js",
                        "~/Scripts/viewmodels/listview.js"));

            bundles.Add(new ScriptBundle("~/js/interview-general").Include(
                "~/Scripts/knockout.validation.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/Math.uuid.js",
                "~/Scripts/viewmodels/pages/interview/custom.js",
                "~/Scripts/lodash.underscore.js"
                ));

            bundles.Add(
                new ScriptBundle("~/js/interview-details").Include(
                    "~/Scripts/moment.js",
                    "~/Scripts/director.js",
                    "~/Scripts/viewmodels/pages/interview/details/config.js",
                    "~/Scripts/viewmodels/pages/interview/details/datacontext.js",
                    "~/Scripts/viewmodels/pages/interview/details/mapper.js",
                    "~/Scripts/viewmodels/pages/interview/details/model.js",
                    "~/Scripts/viewmodels/pages/interview/details/interviewdetailssettings.js",
                    "~/Scripts/viewmodels/pages/interview/details/interviewdetails.js"
                    ));

            bundles.Add(
                new ScriptBundle("~/js/interview-new").Include(
                    "~/Scripts/viewmodels/pages/interview/new/datacontext.js",
                    "~/Scripts/viewmodels/pages/interview/new/mapper.js",
                    "~/Scripts/viewmodels/pages/interview/new/model.js",
                    "~/Scripts/viewmodels/pages/interview/new/newinterview.js"
                    ));

            bundles.Add(new StyleBundle("~/css/interview-new").Include(
             "~/Content/datepicker.css"));


            bundles.Add(new StyleBundle("~/content/bootstrap").Include("~/content/bootstrap.css", "~/content/bootstrap-theme.css"));

            bundles.Add(new StyleBundle("~/content/app").Include("~/content/Application.css", "~/Content/listview.css"));
        }
    }
}