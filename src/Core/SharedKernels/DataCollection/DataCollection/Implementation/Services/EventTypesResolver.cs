using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class EventTypesResolver : IEventTypesResolver
    {
        private readonly Dictionary<string, Type> eventTypeByName = new Dictionary<string, Type>();

        public EventTypesResolver(params Assembly[] assembliesWithEvents)
        {
            var eventInterfaceInfo = typeof(IEvent).GetTypeInfo();

            var eventImplementations = assembliesWithEvents.SelectMany(assembly => assembly.DefinedTypes)
                .Where(definedType => eventInterfaceInfo.IsAssignableFrom(definedType));

            foreach (var eventImplementation in eventImplementations)
            {
                this.eventTypeByName.Add(eventImplementation.Name, eventImplementation.AsType());
                this.eventTypeByName.Add(eventImplementation.FullName, eventImplementation.AsType());
            }
        }

        public Type GetTypeByName(string implementationTypeName) => this.eventTypeByName[implementationTypeName];
    }
}
