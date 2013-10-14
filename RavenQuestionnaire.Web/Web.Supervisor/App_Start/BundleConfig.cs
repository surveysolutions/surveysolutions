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

            bundles.Add(new StyleBundle("~/Content/main").Include(
                "~/Content/css/bootstrap.css",
                "~/Content/font-awesome.min.css",
                "~/Content/bootstrap-mvc-validation.css",
                "~/Content/jquery.pnotify.default.css",
                "~/Content/supervisor.css",
                "~/Content/main.css"));

            bundles.Add(new StyleBundle("~/css/main-not-loggedin").Include(
                "~/Content/css/bootstrap.css",
                "~/Content/bootstrap-mvc-validation.css",
                "~/Content/css/bootstrap-responsive.css",
                 "~/Content/main-not-logged.css"));

            bundles.Add(new ScriptBundle("~/js/main").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/modernizr-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/knockout.mapping-latest.js",
                "~/Scripts/query-string.js",
                "~/Scripts/supervisor.framework.js",
                "~/Scripts/viewmodels/viewmodelbase.js",
                "~/Scripts/viewmodels/pagebase.js"));

            bundles.Add(
                new ScriptBundle("~/validate").Include(
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.validate.unobtrusive-custom-for-bootstrap.js"));

            bundles.Add(new StyleBundle("~/css/list").Include("~/Content/listview.css"));

            bundles.Add(
                new ScriptBundle("~/js/list").Include(
                    "~/Scripts/ko.pager.js",
                    "~/Scripts/viewmodels/listview.js"));

            bundles.Add(new StyleBundle("~/css/interview-new").Include(
                "~/Content/bootstrap-editable.css",
                "~/Content/datepicker.css"));

            bundles.Add(new StyleBundle("~/css/interview").Include(
                "~/Content/bootstrap-editable.css",
                "~/Content/datepicker.css",
                "~/Content/details.css"));

            bundles.Add(
                new ScriptBundle("~/js/interview-general").Include(
                    "~/Scripts/knockout.validation.js",
                    "~/Scripts/bootstrap-datepicker.js",
                    "~/Scripts/Math.uuid.js",
                    "~/Scripts/viewmodels/pages/interview/custom.js"
                    ));

            bundles.Add(
                new ScriptBundle("~/js/interview-details").Include(
                    "~/Scripts/lodash.underscore.js",
                    "~/Scripts/moment.js",
                    "~/Scripts/director.js",
                    "~/Scripts/viewmodels/pages/interview/details/config.js",
                    "~/Scripts/viewmodels/pages/interview/details/datacontext.js",
                    "~/Scripts/viewmodels/pages/interview/details/mapper.js",
                    "~/Scripts/viewmodels/pages/interview/details/model.js",
                    "~/Scripts/viewmodels/pages/interview/details/interviewdetails.js"
                    ));

            bundles.Add(
                new ScriptBundle("~/js/interview-new").Include(
                    "~/Scripts/viewmodels/pages/interview/new/datacontext.js",
                    "~/Scripts/viewmodels/pages/interview/new/mapper.js",
                    "~/Scripts/viewmodels/pages/interview/new/model.js",
                    "~/Scripts/viewmodels/pages/interview/new/newinterview.js"
                    ));
        }
    }
}