using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewGps
    {
        public virtual string InterviewId { get; set; }
        public virtual Guid QuestionId { get; set; }
        public virtual string RosterVector { get; set; }
        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
        public virtual DateTimeOffset Timestamp { get; set; }
        public virtual bool IsEnabled { get; set; }


        public override bool Equals(object obj) => obj is InterviewGps gps &&
                                                   InterviewId == gps.InterviewId &&
                                                   QuestionId.Equals(gps.QuestionId) &&
                                                   RosterVector == gps.RosterVector;

        public override int GetHashCode()
        {
            var hashCode = 435978002;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(InterviewId);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(QuestionId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RosterVector);
            return hashCode;
        }

        public override string ToString() =>
            $"[{Latitude}, {Longitude}] i: {InterviewId} q: {QuestionId}${RosterVector}";
    }
}
