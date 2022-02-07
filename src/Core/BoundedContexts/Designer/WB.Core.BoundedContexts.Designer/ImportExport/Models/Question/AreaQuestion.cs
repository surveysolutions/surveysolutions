
namespace WB.Core.BoundedContexts.Designer.ImportExport.Models.Question
{
    public class AreaQuestion : AbstractQuestion
    {
        public GeometryType? GeometryType { get; set; }
    }
    
    public enum GeometryType
    {
        Polygon = 0,
        Polyline = 1,
        Point = 2,
        Multipoint = 3
    }
}