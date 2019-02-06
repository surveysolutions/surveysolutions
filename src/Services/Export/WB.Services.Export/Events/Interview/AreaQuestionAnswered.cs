using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class AreaQuestionAnswered : QuestionAnswered
    {
        public string Geometry { get; set; }
        public string MapName { get; set; }
        public double? AreaSize { get; set; }
        public double? Length { get; set; }
        public string Coordinates { get; set; }
        public double? DistanceToEditor { get; set; }

        public int? NumberOfPoints { get; set; }
    }
}
