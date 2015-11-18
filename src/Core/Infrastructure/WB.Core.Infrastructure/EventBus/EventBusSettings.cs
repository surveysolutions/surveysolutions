using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.EventBus
{
    public class EventBusSettings
    {
        public Type[] DisabledEventHandlerTypes { get; set; }
        public Type[] EventHandlerTypesWithIgnoredExceptions { get; set; }        
        public HashSet<string> IgnoredAggregateRoots { get; set; }
    }
}
