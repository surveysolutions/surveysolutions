using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewGpsAnswer
    {
        public Guid InterviewId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}