using System;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public abstract class AssignmentEvent : IEvent
    {
        public Guid UserId { get; }
        public DateTimeOffset OriginDate { get; }
        
        protected AssignmentEvent(Guid userId, DateTimeOffset originDate)
        {
            this.UserId = userId;
            this.OriginDate = originDate;
        }
    }
}
