using System.Web.Optimization;

namespace WB.UI.WebTester
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/StylesRuntime").Include(
                "~/Content/Styles/markup.css",
                "~/Content/Styles/markup-web-interview.css"
            ));
        }
    }
}
