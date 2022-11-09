using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class AreaQuestionAnswered : QuestionAnswered
    {
        public string Geometry { get; set; } = String.Empty;
        public string MapName { get; set; } = String.Empty;
        public double? AreaSize { get; set; }
        public double? Length { get; set; }
        public string Coordinates { get; set; } = String.Empty;
        public double? DistanceToEditor { get; set; }
        public double? RequestedAccuracy { get; set; }
        public double? RequestedFrequency { get; set; }

        public int? NumberOfPoints { get; set; }
    }
}
