using System;

namespace WB.Core.BoundedContexts.Headquarters.IntreviewerProfiles
{
    public class InterviewerPoint
    {
        public Guid InterviewId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? Timestamp { get; set; }
        public int Index { get; set; }
    }
}
