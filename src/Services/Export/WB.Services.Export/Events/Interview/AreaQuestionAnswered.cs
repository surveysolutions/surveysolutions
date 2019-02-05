using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
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
    }
}
