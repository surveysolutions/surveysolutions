using GeoJSON.Net.Feature;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class MapReportView
    {
        public double[] InitialBounds { get; set; }
        public FeatureCollection FeatureCollection { get; set; }
        public int TotalPoint { get; set; }
    }
}
