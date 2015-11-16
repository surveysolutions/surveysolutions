using System;

namespace WB.Core.Infrastructure.EventBus
{
    public class EventBusSettings
    {
        public Type[] IgnoredEventHandlerTypes { get; set; }
        public Type[] CatchExceptionsByEventHandlerTypes { get; set; }
    }
}
