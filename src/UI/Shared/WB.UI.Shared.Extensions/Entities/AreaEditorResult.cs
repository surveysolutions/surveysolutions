namespace WB.UI.Shared.Extensions.Entities
{
    public class AreaEditorResult
    {
        public string Geometry { set; get; }
        public string MapName { set; get; }
        public double? Area { set; get; }
        public double? Length { set; get; }
        public string Coordinates { set; get; }

        public int? NumberOfPoints { set; get; }
        public double? DistanceToEditor { set; get; }
        public byte[] Preview { set; get; }
        
        public double? RequestedAccuracy { set; get; }
    }
}
