using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerGeographyQuestionCommand : AnswerQuestionCommand
    {
        public string Geometry { get; private set; }
        public string MapName { get; private set; }
        public double? Area { get; private set; }
        public string Coordinates { set; get; }
        public double? Length { get; private set; }
        public double? DistanceToEditor { get; private set; }

        public int? NumberOfPoints { set; get; }

        public AnswerGeographyQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector,
            string geometry, string mapName, double? area,string coordinates, 
            double? length, double? distanceToEditor, int? numberOfPoints)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.Geometry = geometry;
            this.MapName = mapName;
            this.Area = area;
            this.Length = length;
            this.Coordinates = coordinates;
            this.DistanceToEditor = distanceToEditor;
            this.NumberOfPoints = numberOfPoints;
        }
    }
}
