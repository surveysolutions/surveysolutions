namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public class Area
    {
        public Area(){}

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
            return $"map:{MapName}, size:{AreaSize}, geometry:{Geometry}";
        }
    }
}
