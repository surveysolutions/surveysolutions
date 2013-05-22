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

            bundles.Add(
                new ScriptBundle("~/validate").Include(
                    "~/Scripts/jquery.validate.js", 
                    "~/Scripts/jquery.validate.unobtrusive-custom-for-bootstrap.js"));
            
        }

        #endregion
    }
}