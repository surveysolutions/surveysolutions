using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class GeoLocationQuestionAnswered : QuestionAnswered
    {
        public DateTimeOffset Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
        public double? Altitude { get; set; }
    }
}
