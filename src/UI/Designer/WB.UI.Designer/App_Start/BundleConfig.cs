using System.Web.Optimization;

namespace WB.UI.Designer
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = false;
            //BundleTable.EnableOptimizations = true;

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
                    "~/Scripts/custom/bootstrap3-unobtrusive-hack.js",
                    "~/Content/plugins/bootstrap-select.min.js"));
            bundles.Add(
                new StyleBundle("~/editform-css").Include(new[] {
                    "~/Content/plugins/bootstrap-select.min.css" }));
            bundles.Add(
                new ScriptBundle("~/list").Include(
                    "~/Content/plugins/jquery.mousewheel.js",
                    "~/Content/plugins/perfect-scrollbar.jquery.js",
                    "~/Scripts/custom/common.js",
                    "~/Content/plugins/moment-with-locales.min.js"));
            bundles.Add(
                new ScriptBundle("~/folders").Include(new[] {
                    "~/Content/plugins/jquery.fancytree-all-deps.min.js",
                    "~/Content/plugins/jquery.fancytree.contextMenu.js",
                    "~/Content/plugins/jquery.contextMenu.min.js",
                    "~/Content/plugins/bootbox.min.js",
                    "~/Scripts/custom/public-folders.js" }));
            bundles.Add(
                new StyleBundle("~/Content/plugins/folders-skin").Include( new [] {
                    "~/Content/plugins/ui.fancytree.min.css",
                    "~/Content/plugins/jquery.contextMenu.min.css"
                }));
            bundles.Add(
                new StyleBundle("~/under-construction").Include( new [] {
                    "~/questionnaire/content/designer-start/bootstrap-custom.css",
                    "~/Content/designer-list.css",
                    "~/Content/under-construction.css"
                }));
        }
    }
}
