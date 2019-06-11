namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioAnswerGeographyCommand : ScenarioAnswerCommand
    {
        public ScenarioAnswerGeographyCommand(string variable, RosterVector rosterVector, string geometry, string mapName, double? area, string coordinates, double? length, double? distanceToEditor, int? numberOfPoints) : base(variable, rosterVector)
        {
            Geometry = geometry;
            MapName = mapName;
            Area = area;
            Coordinates = coordinates;
            Length = length;
            DistanceToEditor = distanceToEditor;
            NumberOfPoints = numberOfPoints;
        }

        public string Geometry { get; }
        public string MapName { get; }
        public double? Area { get; }
        public string Coordinates { get; }
        public double? Length { get; }
        public double? DistanceToEditor { get;  }
        public int? NumberOfPoints { get; }
    }
}
