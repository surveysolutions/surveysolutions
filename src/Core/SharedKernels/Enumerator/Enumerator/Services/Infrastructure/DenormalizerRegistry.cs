using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public class DenormalizerRegistry: IDenormalizerRegistry
    {
        private readonly Dictionary<Type, HashSet<BaseDenormalizer>> eventTypes = new Dictionary<Type, HashSet<BaseDenormalizer>>();
        public void RegisterDenormalizer(BaseDenormalizer denormalizer)
        {
            foreach (var eventType in GetRegisteredEvents(denormalizer))
            {
                if (!eventTypes.ContainsKey(eventType))
                    eventTypes[eventType] = new HashSet<BaseDenormalizer>();

                eventTypes[eventType].Add(denormalizer);
            }

        }

        public IReadOnlyCollection<BaseDenormalizer> GetDenormalizers(CommittedEvent @event) =>
            this.eventTypes.ContainsKey(@event.Payload.GetType())
                ? this.eventTypes[@event.Payload.GetType()]
                : new HashSet<BaseDenormalizer>();

        private IEnumerable<Type> GetRegisteredEvents(BaseDenormalizer handler) =>
            handler
                .GetType()
                .GetTypeInfo()
                .ImplementedInterfaces
                .Where(type => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(x => x.GetTypeInfo().GenericTypeArguments.Single());
    }
}
