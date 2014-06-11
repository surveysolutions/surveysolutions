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

            bundles.Add(new StyleImagePathBundle("~/Content/bootstrap-bundle")
                .Include("~/Content/bootstrap/bootstrap.css")
                .Include("~/Content/bootstrap/bootstrap-mvc-validation.css"));

            bundles.Add(new StyleBundle("~/Content/main")
                .Include("~/Content/jquery.pnotify.default.css")
                .Include("~/Content/supervisor.css")
                .Include("~/Content/main.css"))
               ;

            bundles.Add(new StyleBundle("~/css/main-not-loggedin").Include(
                 "~/Content/main-not-logged.css"));

            bundles.Add(new StyleBundle("~/css/list").Include("~/Content/listview.css"));

            bundles.Add(new StyleBundle("~/css/interview-new").Include(
                "~/Content/bootstrap-editable.css",
                "~/Content/datepicker.css"));

            bundles.Add(new StyleBundle("~/css/interview").Include(
                "~/Content/bootstrap-editable.css",
                "~/Content/datepicker.css",
                "~/Content/details.css"));
        }
    }
}