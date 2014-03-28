using System.Web.Optimization;

namespace WB.UI.Headquarters.App_Start
{
    public static class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*", "~/Scripts/ValidationFix.js"));


            bundles.Add(new ScriptBundle("~/bundles/angular").Include("~/Scripts/angular.js", 
                "~/Scripts/angular-*",
                "~/Scripts/ui-bootstrap-{version}.js",
                "~/Scripts/ui-bootstrap-tpls-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/appjs").IncludeDirectory("~/clientSide/controllers", "*.js"));

            bundles.Add(new StyleBundle("~/content/bootstrap").Include("~/content/bootstrap.css", "~/content/bootstrap-theme.css"));
            bundles.Add(new StyleBundle("~/content/app").Include("~/content/Application.css"));
        }
    }
}