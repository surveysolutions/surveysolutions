using System;
using System.Diagnostics;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewGpsAnswer
    {
        public Guid InterviewId { get; set; }
        public string RosterVector { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public override bool Equals(object obj)
        {
            var target = obj as InterviewGpsAnswer;
            if (target == null) return false;

            return this.Equals(target);
        }

        protected bool Equals(InterviewGpsAnswer other) => InterviewId.Equals(other.InterviewId) &&
                                                           Latitude.Equals(other.Latitude) &&
                                                           Longitude.Equals(other.Longitude);

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

    [DebuggerDisplay("{ToString()}")]
    public class InterviewGpsAnswerWithTimeStamp
    {
        public Guid InterviewId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Guid EntityId { get; set; }
        public DateTime? Timestamp { get; set; }
        public InterviewStatus Status { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is InterviewGpsAnswerWithTimeStamp target)) return false;

            return this.Equals(target);
        }

        protected bool Equals(InterviewGpsAnswerWithTimeStamp other)
        {
            return InterviewId.Equals(other.InterviewId) 
                   && Latitude.Equals(other.Latitude) 
                   && Longitude.Equals(other.Longitude) 
                   && EntityId.Equals(other.EntityId) 
                   && Timestamp.Equals(other.Timestamp);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = InterviewId.GetHashCode();
                hashCode = (hashCode * 397) ^ Latitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
                hashCode = (hashCode * 397) ^ EntityId.GetHashCode();
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"lat:{this.Latitude}, long: {this.Longitude}, id: {this.InterviewId}, timestamp: {this.Timestamp}";
    }
}
