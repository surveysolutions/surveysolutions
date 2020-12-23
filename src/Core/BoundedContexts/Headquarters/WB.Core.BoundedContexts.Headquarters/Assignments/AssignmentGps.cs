using System;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentGps : EntityWithTypedId<int>
    {
        public virtual int AssignmentId { get; set; }
        public virtual Guid QuestionId { get; set; }
        public virtual string RosterVector { get; set; }
        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
        public virtual DateTimeOffset Timestamp { get; set; }

        public override string ToString() =>
            $"[{Latitude}, {Longitude}] q: {QuestionId}${RosterVector}";
    }
}