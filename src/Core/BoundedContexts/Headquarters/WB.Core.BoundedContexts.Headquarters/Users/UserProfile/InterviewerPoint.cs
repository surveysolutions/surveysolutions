using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserProfile
{
    public class InterviewerPoint
    {
        public List<Guid> InterviewIds { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public int Index { get; set; }
        public string[] Colors { get; set; }
    }

    public class InterviewerPoints
    {
        public List<InterviewerPoint> CheckInPoints { get; set; } = new List<InterviewerPoint>();
        public List<InterviewerPoint> TargetLocations { get; set; } = new List<InterviewerPoint>();
    }
}
