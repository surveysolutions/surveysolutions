using System;

namespace WB.Core.Infrastructure.EventBus
{
    public abstract class BaseTrackableEvent : IEvent
    {
        //OriginDate could be null for backward compatibility
        //But All newly created events should provide originDate
        protected BaseTrackableEvent(DateTimeOffset? originDate)
        {
            if (originDate != default(DateTimeOffset))
            {
                this.OriginDate = originDate;
            }
        }

        public DateTimeOffset? OriginDate { set; get; }
    }
 }
