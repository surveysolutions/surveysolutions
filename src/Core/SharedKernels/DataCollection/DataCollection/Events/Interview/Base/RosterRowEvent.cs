using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class RosterRowEvent : InterviewPassiveEvent
    {
        public Guid GroupId { get; private set; }
        public decimal[] OuterRosterVector { get; private set; }
        public decimal RosterInstanceId { get; private set; }

        protected RosterRowEvent(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId, DateTimeOffset originDate)
            : base (originDate)
        {
            this.GroupId = groupId;
            this.OuterRosterVector = outerRosterVector;
            this.RosterInstanceId = rosterInstanceId;
        }
    }
}
