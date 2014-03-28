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
                "~/Scripts/viewmodels/viewmodelbase.js",
                 "~/Scripts/viewmodels/pagebase.js"));


            bundles.Add(new ScriptBundle("~/js/list").Include(
                        "~/Scripts/ko.pager.js",
                        "~/Scripts/viewmodels/listview.js"));

            bundles.Add(new StyleBundle("~/content/bootstrap").Include("~/content/bootstrap.css", "~/content/bootstrap-theme.css"));

            bundles.Add(new StyleBundle("~/content/app").Include("~/content/Application.css", "~/Content/listview.css"));
        }
    }
}