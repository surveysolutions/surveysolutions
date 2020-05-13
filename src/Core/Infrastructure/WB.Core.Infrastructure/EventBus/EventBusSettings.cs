using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.CustomCollections;

namespace WB.Core.Infrastructure.EventBus
{
    public class EventBusSettings
    {
        public Type[] DisabledEventHandlerTypes { get; set; } = Array.Empty<Type>();
        public Type[] EventHandlerTypesWithIgnoredExceptions { get; set; } = Array.Empty<Type>();
        private ConcurrentHashSet<string> IgnoredAggregateRoots { get; } = new ConcurrentHashSet<string>();
       
        public void AddIgnoredAggregateRoot(Guid id)
        {
            this.IgnoredAggregateRoots.Add(id.FormatGuid());
        }
        
        public bool IsIgnoredAggregate(Guid id) => this.IgnoredAggregateRoots.Contains(id.FormatGuid(), StringComparer.OrdinalIgnoreCase);

        public void RemoveRoot(Guid id) => this.IgnoredAggregateRoots.Remove(id.FormatGuid());
    }
}
