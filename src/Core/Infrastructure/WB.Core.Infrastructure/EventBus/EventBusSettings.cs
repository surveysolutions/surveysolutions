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
    }
}
