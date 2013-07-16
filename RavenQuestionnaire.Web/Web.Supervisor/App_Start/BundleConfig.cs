namespace Web.Supervisor.App_Start
{
    using System.Web.Optimization;

    /// <summary>
    /// The bundle config.
    /// </summary>
    public class BundleConfig
    {
        #region Public Methods and Operators

        /// <summary>
        /// The register bundles.
        /// </summary>
        /// <param name="bundles">
        /// The bundles.
        /// </param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = false;

            bundles.IgnoreList.Clear();
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*intellisense.js");

            bundles.Add(new StyleBundle("~/css/main").Include(
                "~/Content/bootstrap.css",
                "~/Content/bootstrap.icon-large.min.css",
                "~/Content/bootstrap-responsive.css",
                "~/Content/supervisor.css",
                "~/Content/main.css"));

            bundles.Add(new StyleBundle("~/css/main-not-loggedin").Include(
                "~/Content/bootstrap.css", 
                "~/Content/bootstrap.icon-large.min.css",
                "~/Content/bootstrap-responsive.css",
                 "~/Content/main-not-logged.css"));

            bundles.Add(new ScriptBundle("~/js/main").Include(
                "~/Scripts/jquery-{version}.js", 
                "~/Scripts/bootstrap.js", 
                "~/Scripts/query-string.js"));

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

            bundles.Add(new ScriptBundle("~/js/interview").Include(
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/jquery.validate.unobtrusive.min.js",
                "~/Scripts/jquery-ui-1.10.0.min.js",
                "~/Scripts/knockout-2.2.1.js",
                "~/Scripts/knockout.validation.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/bootstrap-editable.js",
                "~/Scripts/shorten.js",
                "~/Scripts/director.min.js",
                "~/Scripts/date.js", 
                "~/Scripts/details.js"));
        }

        #endregion
    }
}