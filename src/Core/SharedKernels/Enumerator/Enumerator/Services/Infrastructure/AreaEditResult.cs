namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public class AreaEditResult
    {
        public string Geometry { get; set; }
        public string MapName { get; set; }
        public double? Area { get; set; }
        public double? RequestedAccuracy { get; set; }
        public double? Length { set; get; }
        public string Coordinates { set; get; }

        public double? DistanceToEditor { set; get; }

        public int? NumberOfPoints { set; get; }
        public byte[] Preview { set; get; }
        
        public double? RequestedFrequency { set; get; }
    }
}
