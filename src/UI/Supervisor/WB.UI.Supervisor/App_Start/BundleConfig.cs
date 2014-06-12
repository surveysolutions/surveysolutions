using WB.UI.Supervisor.Code.Bundling;

namespace WB.UI.Supervisor.App_Start
{
    using System.Web.Optimization;

    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = false;

            bundles.IgnoreList.Clear();
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*intellisense.js");

            bundles.Add(new StyleBundle("~/Content/main").Include(
                "~/Content/bootstrap.css",
                "~/Content/font-awesome.min.css",
                "~/Content/bootstrap-mvc-validation.css",
                "~/Content/jquery.pnotify.default.css",
                "~/Content/app.css"
                /*"~/Content/supervisor.css",
                "~/Content/main.css"*/));

            bundles.Add(new StyleBundle("~/css/main-not-loggedin").Include(
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-mvc-validation.css",
                "~/Content/main-not-logged.css"));

            bundles.Add(new StyleBundle("~/css/list").Include("~/Content/listview.css"));

            bundles.Add(new StyleBundle("~/css/interview-new").Include(
                "~/Content/bootstrap-editable.css",
                "~/Content/datepicker.css"));

            bundles.Add(new StyleBundle("~/css/interview").Include(
                "~/Content/bootstrap-editable.css",
                "~/Content/datepicker.css"
                /*"~/Content/details.css"*/));
        }
    }
}