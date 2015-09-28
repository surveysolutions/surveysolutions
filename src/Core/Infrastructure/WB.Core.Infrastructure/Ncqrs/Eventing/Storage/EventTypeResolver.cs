using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Ncqrs.Eventing.Storage
{
    public class EventTypeResolver : IEventTypeResolver
    {
        readonly Dictionary<string, Type> KnownEventDataTypes = new Dictionary<string, Type>();

        public Type ResolveType(string eventName)
        {
            return KnownEventDataTypes[eventName];
        }

        public void RegisterEventDataType(Type eventDataType)
        {
            ThrowIfThereIsAnotherEventWithSameFullName(eventDataType);
            ThrowIfThereIsAnotherEventWithSameName(eventDataType);

            KnownEventDataTypes[eventDataType.FullName] = eventDataType;
            KnownEventDataTypes[eventDataType.Name] = eventDataType;
        }

        void ThrowIfThereIsAnotherEventWithSameName(Type @event)
        {
            Type anotherEventWithSameName;
            KnownEventDataTypes.TryGetValue(@event.Name, out anotherEventWithSameName);

            if (anotherEventWithSameName != null && anotherEventWithSameName != @event)
                throw new ArgumentException(string.Format("Two different events share same type name:{0}{1}{0}{2}",
                    Environment.NewLine, @event.AssemblyQualifiedName, anotherEventWithSameName.AssemblyQualifiedName));
        }

        void ThrowIfThereIsAnotherEventWithSameFullName(Type @event)
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
