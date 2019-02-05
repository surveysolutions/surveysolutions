using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class GeoLocationQuestionAnswered : QuestionAnswered
    {
        public DateTimeOffset Timestamp { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double Accuracy { get; private set; }

        public double Altitude { get; private set; }
    }
}
