namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public class Area
    {
        public Area(){}

        public Area(string geometry, string mapName, double? areaSize, double? length, string coordinates, double? distanceToEditor)
        {
            this.Geometry = geometry;
            this.MapName = mapName;
            this.AreaSize = areaSize;
            this.Length = length;
            this.DistanceToEditor = distanceToEditor;
            this.Coordinates = coordinates;
        }

        public string Geometry { set; get; }
        public string MapName { set; get; }
        public double? AreaSize { set; get; }
        public double? Length { set; get; }
        public string Coordinates { set; get; }

        public double? DistanceToEditor { set; get; }


        public override string ToString()
        {
            return Coordinates;
        }
    }
}
