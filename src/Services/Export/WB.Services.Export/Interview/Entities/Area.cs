namespace WB.Services.Export.Interview.Entities
{
    public class Area
    {
        public Area() { }

        public Area(string geometry, string mapName, int? numberOfPoints, double? areaSize,
            double? length, string coordinates, double? distanceToEditor)
        {
            this.Geometry = geometry;
            this.MapName = mapName;
            this.AreaSize = areaSize;
            this.Length = length;
            this.DistanceToEditor = distanceToEditor;
            this.Coordinates = coordinates;
            this.NumberOfPoints = numberOfPoints;
        }

        public string Geometry { set; get; }
        public string MapName { set; get; }
        public double? AreaSize { set; get; }
        public double? Length { set; get; }
        public string Coordinates { set; get; }

        public double? DistanceToEditor { set; get; }

        public int? NumberOfPoints { set; get; }

        public override string ToString()
        {
            return Coordinates;
        }

        public override bool Equals(object obj)
        {
            var target = obj as Area;
            if (target == null) return false;

            return this.Equals(target);
        }

        protected bool Equals(Area other)
            => string.Equals(Geometry, other.Geometry) && string.Equals(MapName, other.MapName) &&
               AreaSize.Equals(other.AreaSize) && Length.Equals(other.Length) &&
               string.Equals(Coordinates, other.Coordinates) && DistanceToEditor.Equals(other.DistanceToEditor);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Geometry != null ? Geometry.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MapName != null ? MapName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ AreaSize.GetHashCode();
                hashCode = (hashCode * 397) ^ Length.GetHashCode();
                hashCode = (hashCode * 397) ^ (Coordinates != null ? Coordinates.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ DistanceToEditor.GetHashCode();
                return hashCode;
            }
        }
    }
}
