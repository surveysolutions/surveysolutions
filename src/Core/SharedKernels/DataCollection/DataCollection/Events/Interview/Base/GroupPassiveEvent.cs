using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class GroupPassiveEvent : InterviewPassiveEvent
    {
        public Guid GroupId { get; private set; }
        public decimal[] RosterVector { get; private set; }

        protected GroupPassiveEvent(Guid groupId, decimal[] rosterVector, DateTimeOffset originDate) : base(originDate)
        {
            this.GroupId = groupId;
            this.RosterVector = rosterVector;
        }
    }
}
