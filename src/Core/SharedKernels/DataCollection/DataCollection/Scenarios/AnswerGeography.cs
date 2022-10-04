namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerGeography : ScenarioAnswerCommand
    {
        public AnswerGeography(string variable, RosterVector rosterVector, string geometry, 
            string mapName, double? area, string coordinates, double? length, 
            double? distanceToEditor, int? numberOfPoints, double? requestedAccuracy) 
            : base(variable, rosterVector)
        {
            Geometry = geometry;
            MapName = mapName;
            Area = area;
            Coordinates = coordinates;
            Length = length;
            DistanceToEditor = distanceToEditor;
            NumberOfPoints = numberOfPoints;
            RequestedAccuracy = requestedAccuracy;
        }

        public string Geometry { get; }
        public string MapName { get; }
        public double? Area { get; }
        public double? RequestedAccuracy { get; }
        public string Coordinates { get; }
        public double? Length { get; }
        public double? DistanceToEditor { get;  }
        public int? NumberOfPoints { get; }
    }
}
