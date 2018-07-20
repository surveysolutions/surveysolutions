using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AreaQuestionAnswered : QuestionAnswered
    {
        public string Geometry { set; get; }
        public string MapName { set; get; }
        public double? AreaSize { set; get; }
        public double? Length { set; get; }
        public string Coordinates { set; get; }
        public double? DistanceToEditor { set; get; }

        public int? NumberOfPoints { set; get; }

        public AreaQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector,
                                    DateTimeOffset originDate, string geometry, string mapName, double? areaSize, 
                                    double? length, string coordinates, double? distanceToEditor, int? numberOfPoints)
            : base(userId, questionId, rosterVector, originDate)
        {
            
            this.Geometry = geometry;
            this.MapName = mapName;
            this.AreaSize = areaSize;
            this.Length = length;
            this.Coordinates = coordinates;
            this.DistanceToEditor = distanceToEditor;
            this.NumberOfPoints = numberOfPoints;
        }
    }
}
