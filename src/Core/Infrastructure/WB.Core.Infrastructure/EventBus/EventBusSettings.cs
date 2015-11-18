using System;

namespace WB.Core.Infrastructure.EventBus
{
    public class EventBusSettings
    {
        public Type[] DisabledEventHandlerTypes { get; set; }
        public Type[] EventHandlerTypesWithIgnoredExceptions { get; set; }
    }
}
