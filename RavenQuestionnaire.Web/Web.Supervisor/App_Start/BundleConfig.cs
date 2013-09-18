namespace Web.Supervisor.App_Start
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

            bundles.Add(new StyleBundle("~/css/main").Include(
                "~/Content/css/bootstrap.css",
                "~/Content/css/bootstrap.icon-large.css",
                "~/Content/bootstrap-mvc-validation.css",
                //"~/Content/css/bootstrap-responsive.css",
                "~/Content/font-awesome.css",
                "~/Content/jquery.pnotify.default.css",
                "~/Content/datepicker.css",
                "~/Content/supervisor.css",
                "~/Content/main.css"));

            bundles.Add(new StyleBundle("~/css/main-not-loggedin").Include(
                "~/Content/css/bootstrap.css",
                "~/Content/css/bootstrap.icon-large.min.css",
                "~/Content/bootstrap-mvc-validation.css",
                "~/Content/css/bootstrap-responsive.css",
                 "~/Content/main-not-logged.css"));

            bundles.Add(new ScriptBundle("~/js/main").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/query-string.js",
                "~/Scripts/lib/jquery.pnotify.js",
                "~/Scripts/bootstrap-datepicker.js"
                ));

            bundles.Add(
                new ScriptBundle("~/validate").Include(
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.validate.unobtrusive-custom-for-bootstrap.js"));

            bundles.Add(new StyleBundle("~/css/list").Include("~/Content/listview.css"));

            bundles.Add(
                new ScriptBundle("~/js/list").Include(
                    "~/Scripts/knockout-2.2.1.js",
                    "~/Scripts/knockout.mapping-latest.js",
                    "~/Scripts/ko.pager.js",
                    "~/Scripts/vm/listview.js"));

            bundles.Add(new StyleBundle("~/css/interview").Include(
                "~/Content/bootstrap-editable.css",
                "~/Content/datepicker.css",
                "~/Content/main.css",
                "~/Content/details.css"));


            bundles.Add(
                new ScriptBundle("~/js/lib-modules").Include(
                    "~/Scripts/lib/lodash.js",
                    "~/Scripts/lib/moment.js",
                    "~/Scripts/lib/jquery.pnotify.js",
                    "~/Scripts/lib/knockout.js",
                    "~/Scripts/lib/knockout.validation.js",
                    "~/Scripts/lib/require.js",
                    "~/Scripts/lib/amplify.js",
                    "~/Scripts/lib/director.js",
                    "~/Scripts/lib/Math.uuid.js"
                    ));

            bundles.Add(
                new ScriptBundle("~/js/interview-details").Include(
                    "~/Scripts/details/app/config.js",
                    "~/Scripts/details/app/datacontext.js",
                    "~/Scripts/details/app/dataservice.js",
                    "~/Scripts/details/app/mapper.js",
                    "~/Scripts/details/app/model.js",
                    "~/Scripts/details/app/viewmodel.js"
                    ));

            bundles.Add(
                new ScriptBundle("~/js/interview-assign").Include(
                    "~/Scripts/assign/app/datacontext.js",
                    "~/Scripts/assign/app/dataservice.js",
                    "~/Scripts/assign/app/mapper.js",
                    "~/Scripts/assign/app/model.js",
                    "~/Scripts/assign/app/viewmodel.js"
                    ));

            bundles.Add(
               new ScriptBundle("~/js/interview-changeState").Include(
                   "~/Scripts/changeState/app/viewmodel.js"
                   ));

        }
    }
}