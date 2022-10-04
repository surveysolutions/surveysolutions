namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class Area
    {
        public Area(string geometry, string mapName, int? numberOfPoints,  
            double? areaSize, double? length, double? distanceToEditor, double? accuracy)
        {
            this.Geometry = geometry;
            this.MapName = mapName;
            this.AreaSize = areaSize;
            this.Length = length;
            this.DistanceToEditor = distanceToEditor;
            this.NumberOfPoints = numberOfPoints;
            this.Accuracy = accuracy;
        }

        public double? Accuracy { get; set; }

        public string Geometry { set; get; }
        public string MapName { set; get; }
        public double? AreaSize { set; get; }
        public double? Length { set; get; }
        public double? DistanceToEditor { set; get; }

        public int? NumberOfPoints { set; get; }

        public override string ToString()
        {
            return string.Format($"map:\"{MapName}\", size:\"{AreaSize}\", geometry:\"{Geometry}\"");
        }
    }

}
