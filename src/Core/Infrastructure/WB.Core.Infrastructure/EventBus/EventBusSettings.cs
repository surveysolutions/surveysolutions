using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.EventBus
{
    public class EventBusSettings
    {
        public EventBusSettings()
        {
            this.IgnoredAggregateRoots=new HashSet<string>();
            this.EventHandlerTypesWithIgnoredExceptions=new Type[0];
            this.DisabledEventHandlerTypes = new Type[0];
        }

        public Type[] DisabledEventHandlerTypes { get; set; }
        public Type[] EventHandlerTypesWithIgnoredExceptions { get; set; }        
        public HashSet<string> IgnoredAggregateRoots { get; set; }
    }
}
