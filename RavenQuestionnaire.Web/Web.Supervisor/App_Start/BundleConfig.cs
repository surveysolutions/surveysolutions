// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BundleConfig.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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

            bundles.Add(new StyleBundle("~/css/main").Include("~/Content/bootstrap.css", "~/Content/bootstrap.icon-large.min.css", "~/Content/bootstrap-responsive.css"));
            bundles.Add(new ScriptBundle("~/js/main").Include("~/Scripts/jquery-{version}.js", "~/Scripts/bootstrap.js"));

            bundles.Add(
                new ScriptBundle("~/validate").Include(
                    "~/Scripts/jquery.validate.js", 
                    "~/Scripts/jquery.validate.unobtrusive-custom-for-bootstrap.js"));

            bundles.Add(
                new ScriptBundle("~/js/list").Include(
                    "~/Scripts/knockout-2.2.1.js",
                    "~/Scripts/knockout.mapping-latest.js",
                    "~/Scripts/ko.pager.js",
                    "~/Scripts/vm/listviewmaster.js",
                    "~/Scripts/vm/listview.js"));
        }

        #endregion
    }
}