using System.Web.Optimization;

namespace WB.UI.Designer
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = false;

            bundles.IgnoreList.Clear();
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*intellisense.js");

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/jquery.validate.unobtrusive-custom-for-bootstrap.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            // 3rd Party JavaScript files
            bundles.Add(new ScriptBundle("~/bundles/jsextlibs")
                //.IncludeDirectory("~/Scripts/lib", "*.js", searchSubdirectories: false));
                            .Include(
                                "~/Scripts/lib/json2.js", // IE7 needs this

                                // jQuery plugins
                                "~/Scripts/lib/activity-indicator.js",
                                "~/Scripts/TrafficCop.js",
                                "~/Scripts/infuser.js", // depends on TrafficCop

                                // Knockout and its plugins
                                "~/Scripts/knockout-{version}.js",
                                "~/Scripts/lib/knockout.activity.js",
                                "~/Scripts/lib/knockout.asyncCommand.js",
                                "~/Scripts/lib/knockout.dirtyFlag.js",
                                "~/Scripts/knockout.validation.debug.js",
                                "~/Scripts/lib/koExternalTemplateEngine.js",
                                "~/Scripts/lib/knockout-sortable.js",

                                // Other 3rd party libraries
                                "~/Scripts/lodash.js",
                                "~/Scripts/moment.js",
                                "~/Scripts/lib/sammy.js",
                                "~/Scripts/lib/sammy.title.js",
                                "~/Scripts/amplify.*",
                                "~/Scripts/jquery.pnotify.js",
                                "~/Scripts/bootbox.js",

                                //Plugins
                                "~/Scripts/lib/jquery.autogrow-textarea.js",
                                "~/Scripts/lib/Math.uuid.js"
                            ));

            bundles.Add(new ScriptBundle("~/bundles/jsappdetails")
                   .IncludeDirectory("~/Scripts/details/", "*.js", searchSubdirectories: false));

            bundles.Add(new StyleBundle("~/Content/edit").Include(
               "~/Content/details.css",
               "~/Content/jquery.pnotify.css",
               "~/Content/jquery.pnotify.icons.css"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js"
                ));

            var bundle = new ScriptBundle("~/bundles/ace").Include("~/Scripts/lib/ace/ace.js",
                //"~/Scripts/lib/ace/mode-ncalc.js", 
                //"~/Scripts/lib/ace/theme-designer.js", 
"~/Scripts/lib/ace/knockout-ace.js");
            bundle.Transforms.Clear();
            bundles.Add(bundle);

            bundles.Add(new StyleBundle("~/content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/body.css",
                "~/Content/bootstrap-responsive.css",
                "~/Content/bootstrap-mvc-validation.css"
                ));
        }
    }
}