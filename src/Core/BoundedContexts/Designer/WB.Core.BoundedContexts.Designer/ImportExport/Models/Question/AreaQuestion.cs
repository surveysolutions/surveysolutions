
namespace WB.Core.BoundedContexts.Designer.ImportExport.Models.Question
{
    public class AreaQuestion : AbstractQuestion
    {
        public GeometryType? GeometryType { get; set; }
        public GeometryInputMode? GeometryInputMode { get; set; }
        
        public bool? GeometryOverlapDetection { get; set; }
    }
    
    public enum GeometryType
    {
        Polygon = 0,
        Polyline = 1,
        Point = 2,
        Multipoint = 3
    }
    
    public enum GeometryInputMode
    {
        Manual = 0,
        Automatic = 1,
        Semiautomatic = 2
    }
}
