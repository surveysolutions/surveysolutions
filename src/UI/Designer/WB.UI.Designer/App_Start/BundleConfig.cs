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
                new StyleBundle("~/content/css").Include(
                    "~/questionnaire/vendor/bootstrap/dist/bootstrap.css",
                    "~/Content/font-awesome.min.css",
                    "~/Content/body.css", 
                    "~/Content/bootstrap-responsive.css", 
                    "~/Content/bootstrap-mvc-validation.css"));

            bundles.Add(
                new ScriptBundle("~/simplepage").Include(
                    "~/questionnaire/vendor/jquery/dist/jquery.js",
                    "~/questionnaire/vendor/bootstrap/dist/bootstrap.js"));

            bundles.Add(
                new ScriptBundle("~/editform").Include(
                    "~/Scripts/jquery.validate.js", 
                    "~/Scripts/jquery.validate.unobtrusive-custom-for-bootstrap.js", 
                    "~/Scripts/bootstrap3-unobtrusive-hack.js",
                    "~/questionnaire/vendor/jquery-placeholder/jquery.placeholder.js",
                    "~/Scripts/editForm.js"
                    ));

            bundles.Add(
                new ScriptBundle("~/list").Include(
                    "~/questionnaire/vendor/jquery-mousewheel/jquery.mousewheel.js",
                    "~/questionnaire/vendor/perfect-scrollbar/src/perfect-scrollbar.js",
                    "~/Scripts/common.js"));

            bundles.Add(
                new ScriptBundle("~/readsidebundle").Include(
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/lodash.underscore.js",
                    "~/Scripts/moment.js",
                    "~/Scripts/components/moment-duration-format.js",
                    "~/Scripts/components/bindings.js",
                    "~/Scripts/viewmodels/designer.framework.js",
                    "~/Scripts/viewmodels/viewmodelbase.js",
                    "~/Scripts/viewmodels/pagebase.js",
                    "~/Scripts/viewmodels/pages/controlpanel/readside.js"));

            bundles.Add(
                new ScriptBundle("~/login").Include(
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/lodash.underscore.js",
                    "~/Scripts/components/query-string.js",
                    "~/Scripts/viewmodels/designer.framework.js",
                    "~/Scripts/viewmodels/viewmodelbase.js",
                    "~/Scripts/viewmodels/pagebase.js",
                    "~/Scripts/viewmodels/pages/login.js"));
        }
    }
}