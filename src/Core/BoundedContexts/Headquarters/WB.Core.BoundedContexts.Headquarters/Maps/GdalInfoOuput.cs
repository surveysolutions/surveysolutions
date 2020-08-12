namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public class GdalInfoOuput
    {
        public double[] GeoTransform { get; set; }

        public double[] Size { get; set; }

        public CoordinateSystem CoordinateSystem { get; set; }
        
        public CornerCoordinates CornerCoordinates { get; set; }
        
        public Wgs84Extent Wgs84Extent { get; set; }
    }

    public class Wgs84Extent
    {
        public string Type { get; set; }
        public double[][][] Coordinates { get; set; } 
    }
    
    public class CoordinateSystem
    {
        public string Wkt { get; set; }
    }

    public class CornerCoordinates
    {
        public double[] UpperLeft { get; set; }
        public double[] LowerLeft { get; set; }
        public double[] LowerRight { get; set; }
        public double[] UpperRight { get; set; }
        public double[] Center { get; set; }
    }
}
