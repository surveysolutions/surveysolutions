using System;
using System.Diagnostics;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewGpsInfo
    {
        public Guid InterviewId { get; set; }
        public string InterviewKey { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public InterviewStatus Status { get; set; }

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
            unchecked
            {
                var hashCode = InterviewId.GetHashCode();
                hashCode = (hashCode * 397) ^ Latitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"lat:{this.Latitude}, long: {this.Longitude}, id: {this.InterviewId}";
    }
}