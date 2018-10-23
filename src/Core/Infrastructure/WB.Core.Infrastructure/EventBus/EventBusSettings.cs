using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.Infrastructure.EventBus
{
    public class EventBusSettings
    {
        public EventBusSettings()
        {
            this.IgnoredAggregateRoots = new List<string>();
            this.EventHandlerTypesWithIgnoredExceptions=new Type[0];
            this.DisabledEventHandlerTypes = new Type[0];
        }

        public Type[] DisabledEventHandlerTypes { get; set; }
        public Type[] EventHandlerTypesWithIgnoredExceptions { get; set; }        
        public List<string> IgnoredAggregateRoots { get; set; }

        public bool IsIgnoredAggregate(Guid id)
        {
            return this.IgnoredAggregateRoots.Contains(id.FormatGuid(), StringComparer.OrdinalIgnoreCase);
        }
    }
}
