namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public class GeospatialConfig
    {
        public string GdalHome { get; set; }
        public int GeoJsonMaxSize { get; set; } = 5 * 1024 * 1024 * 8;
    }
}
