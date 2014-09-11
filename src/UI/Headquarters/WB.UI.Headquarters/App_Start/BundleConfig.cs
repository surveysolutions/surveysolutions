using System.Web.Optimization;

namespace WB.UI.Headquarters
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = false;

            bundles.IgnoreList.Clear();
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*intellisense.js");

            bundles.Add(new ScriptBundle("~/Scripts/jqplot-area")
                .Include(
                "~/Scripts/query-string.js"
                ,"~/Scripts/jqPlot/jquery.jqplot.js"
                ,"~/Scripts/jqPlot/plugins/jqplot.dateAxisRenderer.js"
                ,"~/Scripts/jqPlot/plugins/jqplot.highlighter.min.js"
                , "~/Scripts/jqPlot/plugins/jqplot.cursor.js"
                , "~/Scripts/jqPlot/plugins/jqplot.enhancedLegendRenderer.js"
                ));
        }
    }
}