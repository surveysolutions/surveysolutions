using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ncqrs.Eventing.Storage
{
    public class EventTypeResolver : IEventTypeResolver
    {
        private readonly Dictionary<string, Type> KnownEventDataTypes = new Dictionary<string, Type>();

        public EventTypeResolver(params Assembly[] assembliesWithEvents)
        {
            foreach (Assembly assembly in assembliesWithEvents)
            {
                this.RegisterEventDataTypes(assembly);
            }
        }

        private void RegisterEventDataTypes(Assembly assembly)
        {
            var eventInterfaceInfo = typeof(WB.Core.Infrastructure.EventBus.IEvent).GetTypeInfo();

            var eventImplementations = assembly.DefinedTypes.Where(definedType => eventInterfaceInfo.IsAssignableFrom(definedType));

            foreach (var eventImplementation in eventImplementations)
            {
                this.RegisterEventDataType(eventImplementation.AsType());
            }
        }

        public Type ResolveType(string eventName)
        {
            if (KnownEventDataTypes.TryGetValue(eventName, out var type))
            {
                return type;
            }

            throw new ArgumentException($"There is no event with name {eventName} registered", nameof(eventName));
        }

        internal void RegisterEventDataType(Type eventDataType)
        {
            ThrowIfThereIsAnotherEventWithSameFullName(eventDataType);
            ThrowIfThereIsAnotherEventWithSameName(eventDataType);

            KnownEventDataTypes[eventDataType.FullName] = eventDataType;
            KnownEventDataTypes[eventDataType.Name] = eventDataType;
        }

        private void ThrowIfThereIsAnotherEventWithSameName(Type @event)
        {
            KnownEventDataTypes.TryGetValue(@event.Name, out var anotherEventWithSameName);

            if (anotherEventWithSameName != null && anotherEventWithSameName != @event)
                throw new ArgumentException(string.Format("Two different events share same type name:{0}{1}{0}{2}",
                    Environment.NewLine, @event.AssemblyQualifiedName, anotherEventWithSameName.AssemblyQualifiedName));
        }

        private void ThrowIfThereIsAnotherEventWithSameFullName(Type @event)
        {
            Type anotherEventWithSameName;
            KnownEventDataTypes.TryGetValue(@event.FullName, out anotherEventWithSameName);

            if (anotherEventWithSameName != null && anotherEventWithSameName != @event)
                throw new ArgumentException(string.Format(
                    "Two different events share same full type name:{0}{1}{0}{2}",
                    Environment.NewLine, @event.AssemblyQualifiedName, anotherEventWithSameName.AssemblyQualifiedName));
        }
    }
}
