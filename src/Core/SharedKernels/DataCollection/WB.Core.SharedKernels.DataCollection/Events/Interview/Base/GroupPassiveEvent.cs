using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class GroupPassiveEvent : InterviewPassiveEvent
    {
        public Guid GroupId { get; private set; }
        public int[] PropagationVector { get; private set; }

        protected GroupPassiveEvent(Guid groupId, int[] propagationVector)
        {
            this.GroupId = groupId;
            this.PropagationVector = propagationVector;
        }
    }
}