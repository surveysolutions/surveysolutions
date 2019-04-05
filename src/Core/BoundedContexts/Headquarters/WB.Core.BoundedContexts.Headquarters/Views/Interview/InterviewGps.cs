using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewGps
    {
        public virtual string Id { get; set; }
        public virtual string InterviewId { get; set; }
        public virtual Guid QuestionId { get; set; }
        public virtual string RosterVector { get; set; }
        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
        public virtual DateTimeOffset Timestamp { get; set; }
        public virtual bool IsEnabled { get; set; }

        public override bool Equals(object obj) => obj is InterviewGps gps && Id == gps.Id;
        public override int GetHashCode() => 2108858624 + EqualityComparer<string>.Default.GetHashCode(Id);

        public override string ToString() =>
            $"[{Latitude}, {Longitude}] i: {InterviewId} q: {QuestionId}${RosterVector}";
    }
}
