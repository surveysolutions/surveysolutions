using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.IntreviewerProfiles
{
    public class InterviewerPoint
    {
        public List<Guid> InterviewIds { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? Timestamp { get; set; }
        public int Index { get; set; }
        public string[] Colors { get; set; }
    }
}
