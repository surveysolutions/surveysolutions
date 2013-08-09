using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class GroupPassiveEvent : InterviewPassiveEvent
    {
        public Guid GroupId { get; private set; }

        protected GroupPassiveEvent(Guid groupId)
        {
            this.GroupId = groupId;
        }
    }
}