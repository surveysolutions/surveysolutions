using System.Web.Optimization;

namespace WB.UI.Designer
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = false;

            bundles.IgnoreList.Clear();
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*intellisense.js");

            bundles.Add(
                new StyleBundle("~/Content/css-list").Include(
                    "~/Content/plugins/perfect-scrollbar.css",
                    "~/Content/designer-list.css"));

            bundles.Add(
                new StyleBundle("~/questionnaire/content/designer-start/css-list").Include(
                    "~/Content/plugins/perfect-scrollbar.css",
                    "~/questionnaire/content/designer-start/bootstrap-custom.css"
                    ));

            bundles.Add(
                new ScriptBundle("~/simplepage").Include(
                    "~/Content/plugins/jquery.min.js",
                    "~/Content/plugins/bootstrap.min.js"));

            bundles.Add(
                new ScriptBundle("~/editform").Include(
                    "~/Content/plugins/jquery.validate.js",
                    "~/Scripts/custom/jquery.validate.unobtrusive-custom-for-bootstrap.js",
                    "~/Scripts/custom/bootstrap3-unobtrusive-hack.js"));
            bundles.Add(
                new ScriptBundle("~/list").Include(
                    "~/Content/plugins/jquery.mousewheel.js",
                    "~/Content/plugins/perfect-scrollbar.jquery.js",
                    "~/Scripts/custom/common.js"));
        }
    }
}