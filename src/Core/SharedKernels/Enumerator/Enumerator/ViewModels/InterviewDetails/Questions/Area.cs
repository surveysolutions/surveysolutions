namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class Area
    {
        public Area(string geometry, string mapName, double areaSize)
        {
            this.Geometry = geometry;
            this.MapName = mapName;
            this.AreaSize = areaSize;
        }

        public string Geometry { set; get; }
        public string MapName { set; get; }
        public double AreaSize { set; get; }

        public override string ToString()
        {
            return string.Format($"map:\"{MapName}\", size:\"{AreaSize}\", geometry:\"{Geometry}\"");
        }
    }

}
