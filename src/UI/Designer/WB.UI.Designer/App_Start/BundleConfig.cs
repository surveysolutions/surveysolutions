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
                    "~/questionnaire/vendor/perfect-scrollbar/css/perfect-scrollbar.css",
                    "~/Content/designer-list.css"));

            bundles.Add(
                new StyleBundle("~/questionnaire/bootstrap/custom/css-list").Include(
                    "~/questionnaire/content/designer-start/bootstrap-custom.css"));

            bundles.Add(
                new StyleBundle("~/content/css").Include(
                    "~/questionnaire/vendor/bootstrap/dist/bootstrap.css",
                    "~/Content/font-awesome.min.css",
                    "~/Content/body.css", 
                    "~/Content/bootstrap-responsive.css", 
                    "~/Content/bootstrap-mvc-validation.css"));

            bundles.Add(
                new ScriptBundle("~/simplepage").Include(
                    "~/questionnaire/vendor/jquery/dist/jquery.min.js",
                    "~/questionnaire/vendor/bootstrap/dist/bootstrap.js"));

            bundles.Add(
                new ScriptBundle("~/editform").Include(
                    "~/questionnaire/vendor/jquery-validation/dist/jquery.validate.js",
                    "~/Scripts/custom/jquery.validate.unobtrusive-custom-for-bootstrap.js",
                    "~/Scripts/custom/bootstrap3-unobtrusive-hack.js",
                    "~/questionnaire/vendor/jquery-placeholder/jquery.placeholder.js",
                    "~/Scripts/custom/editForm.js"));
            bundles.Add(
                new ScriptBundle("~/list").Include(
                    "~/questionnaire/vendor/jquery-mousewheel/jquery.mousewheel.js",
                    "~/questionnaire/vendor/perfect-scrollbar/js/perfect-scrollbar.jquery.js",
                    "~/Scripts/custom/common.js"));
        }
    }
}