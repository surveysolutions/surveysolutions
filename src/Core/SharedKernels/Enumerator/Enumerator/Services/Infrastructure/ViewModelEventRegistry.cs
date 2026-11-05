using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public class ViewModelEventRegistry : IViewModelEventRegistry
    {
        private readonly ILogger logger;

        private readonly object sync = new object();

        private readonly Dictionary<Type, Dictionary<string, HashSet<IViewModelEventHandler>>> eventTypes =
            new Dictionary<Type, Dictionary<string, HashSet<IViewModelEventHandler>>>();

        // Entity-specific handlers: eventType → interviewId → entityId → handlers
        private readonly Dictionary<Type, Dictionary<string, Dictionary<string, HashSet<IViewModelEventHandler>>>> entityEventTypes =
            new Dictionary<Type, Dictionary<string, Dictionary<string, HashSet<IViewModelEventHandler>>>>();

        private readonly Dictionary<IViewModelEventHandler, HashSet<(Type EventType, string AggregateRootId, string EntityId)>> subscriptionsByHandler =
            new Dictionary<IViewModelEventHandler, HashSet<(Type, string, string)>>();

        private readonly ConcurrentDictionary<(Type, Type), MethodInfo> asyncViewModelHandleMethods =
            new ConcurrentDictionary<(Type, Type), MethodInfo>();


        public ViewModelEventRegistry(ILogger logger)
        {
            this.logger = logger;
        }

        public void WriteToLogInfoBySubscribers()
        {
            lock (this.sync)
            {
                var vm = this.eventTypes.SelectMany(et => et.Value.SelectMany(e => e.Value)).ToList();

                logger.Debug("Count of active subscribers: " + vm.Count);
                logger.Debug("Active subscribers: " + string.Join(", ", vm.Select(e => e.GetType().Name).Distinct()));
                logger.Debug("Same identities: " + String.Join(", ", vm.OfType<IInterviewEntityViewModel>().Select(s => s.Identity?.ToString()).Distinct()));
            }
        }
        
        public void Subscribe(IViewModelEventHandler handler, string aggregateRootId)
        {
            lock (this.sync)
            {
                foreach (var eventType in this.GetRegisteredEvents(handler))
                {
                    if (!this.eventTypes.ContainsKey(eventType))
                        this.eventTypes[eventType] = new Dictionary<string, HashSet<IViewModelEventHandler>>();

                    if (!this.eventTypes[eventType].ContainsKey(aggregateRootId))
                        this.eventTypes[eventType][aggregateRootId] = new HashSet<IViewModelEventHandler>();

                    this.eventTypes[eventType][aggregateRootId].Add(handler);
                    this.AddSubscription(handler, (eventType, aggregateRootId, null));
                }
            }
        }

        public void Subscribe(IViewModelEventHandler handler, string aggregateRootId, Identity entityIdentity)
        {
            var entityId = entityIdentity.ToString();
            lock (this.sync)
            {
                foreach (var eventType in this.GetRegisteredEvents(handler))
                {
                    if (!entityEventTypes.ContainsKey(eventType))
                        entityEventTypes[eventType] = new Dictionary<string, Dictionary<string, HashSet<IViewModelEventHandler>>>();

                    if (!entityEventTypes[eventType].ContainsKey(aggregateRootId))
                        entityEventTypes[eventType][aggregateRootId] = new Dictionary<string, HashSet<IViewModelEventHandler>>();

                    if (!entityEventTypes[eventType][aggregateRootId].ContainsKey(entityId))
                        entityEventTypes[eventType][aggregateRootId][entityId] = new HashSet<IViewModelEventHandler>();

                    entityEventTypes[eventType][aggregateRootId][entityId].Add(handler);
                    this.AddSubscription(handler, (eventType, aggregateRootId, entityId));
                }
            }
        }

        public void Unsubscribe(IViewModelEventHandler handler)
        {
            lock (this.sync)
            {
                if (!this.subscriptionsByHandler.TryGetValue(handler, out var subscriptions))
                    return;

                foreach (var subscription in subscriptions.ToArray())
                    this.RemoveSubscription(handler, subscription);
            }
        }

        public bool IsSubscribed(IViewModelEventHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (this.sync)
                return this.subscriptionsByHandler.ContainsKey(handler);
        }

        public IReadOnlyCollection<IViewModelEventHandler> GetViewModelsByEvent(CommittedEvent @event)
        {
            lock (this.sync)
            {
                var eventType = @event.Payload.GetType();
                var eventSourceId = @event.EventSourceId.ToString("N");

                HashSet<IViewModelEventHandler> result = null;

                // Always include non-entity-specific handlers (old behavior)
                if (this.eventTypes.TryGetValue(eventType, out var byInterview) &&
                    byInterview.TryGetValue(eventSourceId, out var globalHandlers) &&
                    globalHandlers.Count > 0)
                {
                    result = new HashSet<IViewModelEventHandler>(globalHandlers);
                }

                // Additionally include entity-specific handlers for affected entities
                if (@event.Payload is IEventWithAffectedEntities withEntities &&
                    this.entityEventTypes.TryGetValue(eventType, out var entityByInterview) &&
                    entityByInterview.TryGetValue(eventSourceId, out var byEntity))
                {
                    foreach (var entityId in withEntities.GetAffectedEntities())
                    {
                        if (byEntity.TryGetValue(entityId.ToString(), out var entityHandlers) && entityHandlers.Count > 0)
                        {
                            if (result == null)
                                result = new HashSet<IViewModelEventHandler>(entityHandlers);
                            else
                                result.UnionWith(entityHandlers);
                        }
                    }
                }

                return result ?? (IReadOnlyCollection<IViewModelEventHandler>) Array.Empty<IViewModelEventHandler>();
            }
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
            lock (this.sync)
            {
                this.eventTypes.Clear();
                this.entityEventTypes.Clear();
                this.subscriptionsByHandler.Clear();
            }
        }

        public void RemoveAggregateRoot(string aggregateRootId)
        {
            lock (this.sync)
            {
                var subscriptions = this.subscriptionsByHandler
                    .SelectMany(handler => handler.Value
                        .Where(subscription => subscription.AggregateRootId == aggregateRootId)
                        .Select(subscription => (Handler: handler.Key, Subscription: subscription)))
                    .ToArray();

                foreach (var subscription in subscriptions)
                    this.RemoveSubscription(subscription.Handler, subscription.Subscription);
            }
        }

        private void AddSubscription(IViewModelEventHandler handler, (Type EventType, string AggregateRootId, string EntityId) subscription)
        {
            if (!this.subscriptionsByHandler.TryGetValue(handler, out var subscriptions))
            {
                subscriptions = new HashSet<(Type, string, string)>();
                this.subscriptionsByHandler[handler] = subscriptions;
            }

            subscriptions.Add(subscription);
        }

        private void RemoveSubscription(IViewModelEventHandler handler, (Type EventType, string AggregateRootId, string EntityId) subscription)
        {
            if (subscription.EntityId == null)
            {
                if (this.eventTypes.TryGetValue(subscription.EventType, out var byInterview) &&
                    byInterview.TryGetValue(subscription.AggregateRootId, out var handlers))
                {
                    handlers.Remove(handler);
                    if (handlers.Count == 0)
                        byInterview.Remove(subscription.AggregateRootId);
                    if (byInterview.Count == 0)
                        this.eventTypes.Remove(subscription.EventType);
                }
            }
            else if (this.entityEventTypes.TryGetValue(subscription.EventType, out var entityByInterview) &&
                     entityByInterview.TryGetValue(subscription.AggregateRootId, out var byEntity) &&
                     byEntity.TryGetValue(subscription.EntityId, out var entityHandlers))
            {
                entityHandlers.Remove(handler);
                if (entityHandlers.Count == 0)
                    byEntity.Remove(subscription.EntityId);
                if (byEntity.Count == 0)
                    entityByInterview.Remove(subscription.AggregateRootId);
                if (entityByInterview.Count == 0)
                    this.entityEventTypes.Remove(subscription.EventType);
            }

            if (this.subscriptionsByHandler.TryGetValue(handler, out var subscriptions))
            {
                subscriptions.Remove(subscription);
                if (subscriptions.Count == 0)
                    this.subscriptionsByHandler.Remove(handler);
            }
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
