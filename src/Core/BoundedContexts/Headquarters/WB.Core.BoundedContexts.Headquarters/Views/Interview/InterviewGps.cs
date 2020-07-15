using System;
using System.Diagnostics;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewGps : EntityWithTypedId<int>
    {
        public virtual InterviewSummary InterviewSummary { get; set; }
        public virtual Guid QuestionId { get; set; }
        public virtual string RosterVector { get; set; }
        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
        public virtual DateTimeOffset Timestamp { get; set; }
        public virtual bool IsEnabled { get; set; }

        public override string ToString() =>
            $"[{Latitude}, {Longitude}] q: {QuestionId}${RosterVector}";
    }
}
