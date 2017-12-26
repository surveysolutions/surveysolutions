using System.Web.Optimization;

namespace WB.UI.WebTester
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/js").Include(
                "~/Content/Scripts/*.js"
            ));

            bundles.Add(new StyleBundle("~/css").Include(
                "~/Content/Styles/markup.css",
                "~/Content/Styles/markup-web-interview.css"
            ));
        }
    }
}
