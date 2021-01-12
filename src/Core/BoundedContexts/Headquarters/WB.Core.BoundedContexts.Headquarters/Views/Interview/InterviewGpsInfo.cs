using System;
using System.Diagnostics;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewGpsInfo
    {
        public Guid InterviewId { get; init; }
        public string InterviewKey { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public InterviewStatus Status { get; init; }

        public override bool Equals(object obj)
        {
            if (!(obj is InterviewGpsInfo target)) return false;

            return this.Equals(target);
        }

        protected bool Equals(InterviewGpsInfo other)
        {
            return InterviewId.Equals(other.InterviewId) 
                   && Latitude.Equals(other.Latitude) 
                   && Longitude.Equals(other.Longitude);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(InterviewId, Latitude, Longitude);
        }

        public override string ToString() => $"lat:{this.Latitude}, long: {this.Longitude}, id: {this.InterviewId}";
    }
}
