using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public class ViewModelEventRegistry : IViewModelEventRegistry
    {
        private Dictionary<Type, Dictionary<string, HashSet<IViewModelEventHandler>>> eventTypes =
            new Dictionary<Type, Dictionary<string, HashSet<IViewModelEventHandler>>>();

        private readonly ConcurrentDictionary<(Type, Type), MethodInfo> asyncViewModelHandleMethods =
            new ConcurrentDictionary<(Type, Type), MethodInfo>();

        public void Subscribe(IViewModelEventHandler handler, string aggregateRootId)
        {
            lock (this.eventTypes)
            {
                foreach (var eventType in this.GetRegisteredEvents(handler))
                {
                    if (!eventTypes.ContainsKey(eventType))
                        this.eventTypes[eventType] = new Dictionary<string, HashSet<IViewModelEventHandler>>();

                    if(!eventTypes[eventType].ContainsKey(aggregateRootId))
                        this.eventTypes[eventType][aggregateRootId] = new HashSet<IViewModelEventHandler>();

                    if (!eventTypes[eventType][aggregateRootId].Contains(handler))
                        this.eventTypes[eventType][aggregateRootId].Add(handler);
                }
            }
        }

        public void Unsubscribe(IViewModelEventHandler handler)
        {
            lock (this.eventTypes)
            {
                foreach (var eventType in this.eventTypes)
                foreach (var aggregateRoot in eventType.Value)
                {
                    if (aggregateRoot.Value.Contains(handler))
                        aggregateRoot.Value.Remove(handler);
                }
            }
        }

        public bool IsSubscribed(IViewModelEventHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            return this.eventTypes.Any(type => type.Value.Any(agRoots => agRoots.Value.Contains(handler)));
        }

        public IReadOnlyCollection<IViewModelEventHandler> GetViewModelsByEvent(CommittedEvent @event)
        {
            var eventType = @event.Payload.GetType();
            var eventSourceId = @event.EventSourceId.ToString("N");

            if (!this.eventTypes.ContainsKey(eventType)) return new IViewModelEventHandler[0];
            if(!this.eventTypes[eventType].ContainsKey(eventSourceId)) return new IViewModelEventHandler[0];

            return this.eventTypes[eventType][eventSourceId].ToList();
        }

        public MethodInfo GetViewModelHandleMethod(Type viewModelType, Type eventType) =>
            this.asyncViewModelHandleMethods.GetOrAdd((viewModelType, eventType), x =>
            {
                var isAsync = x.Item1.GetTypeInfo().ImplementedInterfaces
                    .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncViewModelEventHandler<>))
                    .Any(type => type.GetTypeInfo().GenericTypeArguments.Single() == x.Item2);

                var methodName = $"Handle{(isAsync ? "Async" : "")}";

                return viewModelType.GetRuntimeMethod(methodName, new[] { eventType });
            });

        public void Reset()
        {
            lock (this.eventTypes)
            {
                this.eventTypes = new Dictionary<Type, Dictionary<string, HashSet<IViewModelEventHandler>>>();
            }
        }

        public void RemoveAggregateRoot(string aggregateRootId)
        {
            foreach (var eventType in this.eventTypes)
                eventType.Value.Remove(aggregateRootId);
        }

        private IEnumerable<Type> GetRegisteredEvents(IViewModelEventHandler handler) =>
            handler
                .GetType()
                .GetTypeInfo()
                .ImplementedInterfaces
                .Where(type =>
                    type.GetTypeInfo().IsGenericType &&
                    (type.GetGenericTypeDefinition() == typeof(IViewModelEventHandler<>) ||
                    type.GetGenericTypeDefinition() == typeof(IAsyncViewModelEventHandler<>)))
                .Select(x => x.GetTypeInfo().GenericTypeArguments.Single());
    }
}
