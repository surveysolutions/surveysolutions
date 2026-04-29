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

        private Dictionary<Type, Dictionary<string, HashSet<IViewModelEventHandler>>> eventTypes =
            new Dictionary<Type, Dictionary<string, HashSet<IViewModelEventHandler>>>();

        // Entity-specific handlers: eventType → interviewId → entityId → handlers
        private Dictionary<Type, Dictionary<string, Dictionary<string, HashSet<IViewModelEventHandler>>>> entityEventTypes =
            new Dictionary<Type, Dictionary<string, Dictionary<string, HashSet<IViewModelEventHandler>>>>();

        private readonly ConcurrentDictionary<(Type, Type), MethodInfo> asyncViewModelHandleMethods =
            new ConcurrentDictionary<(Type, Type), MethodInfo>();


        public ViewModelEventRegistry(ILogger logger)
        {
            this.logger = logger;
        }

        public void WriteToLogInfoBySubscribers()
        {
            var vm = eventTypes.SelectMany(et => et.Value.SelectMany(e => e.Value)).ToList();

            logger.Debug("Count of active subscribers: " + vm.Count);
            logger.Debug("Active subscribers: " + string.Join(", ", vm.Select(e => e.GetType().Name).Distinct()));
            logger.Debug("Same identities: " + String.Join(", ", vm.OfType<IInterviewEntityViewModel>().Select(s => s.Identity?.ToString()).Distinct()));
        }
        
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

        public void Subscribe(IViewModelEventHandler handler, string aggregateRootId, Identity entityIdentity)
        {
            var entityId = entityIdentity.ToString();
            lock (this.entityEventTypes)
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

            lock (this.entityEventTypes)
            {
                foreach (var eventType in this.entityEventTypes)
                foreach (var aggregateRoot in eventType.Value)
                foreach (var entityBucket in aggregateRoot.Value)
                {
                    if (entityBucket.Value.Contains(handler))
                        entityBucket.Value.Remove(handler);
                }
            }
        }

        public bool IsSubscribed(IViewModelEventHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (this.eventTypes.Any(type => type.Value.Any(agRoots => agRoots.Value.Contains(handler))))
                return true;

            return this.entityEventTypes.Any(type =>
                type.Value.Any(agRoots =>
                    agRoots.Value.Any(entities => entities.Value.Contains(handler))));
        }

        public IReadOnlyCollection<IViewModelEventHandler> GetViewModelsByEvent(CommittedEvent @event)
        {
            var eventType = @event.Payload.GetType();
            var eventSourceId = @event.EventSourceId.ToString("N");

            List<IViewModelEventHandler> result = null;

            // Always include non-entity-specific handlers (old behavior)
            if (this.eventTypes.TryGetValue(eventType, out var byInterview) &&
                byInterview.TryGetValue(eventSourceId, out var globalHandlers) &&
                globalHandlers.Count > 0)
            {
                result = new List<IViewModelEventHandler>(globalHandlers);
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
                            result = new List<IViewModelEventHandler>(entityHandlers);
                        else
                            result.AddRange(entityHandlers);
                    }
                }
            }

            return result ?? (IReadOnlyCollection<IViewModelEventHandler>) Array.Empty<IViewModelEventHandler>();
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
            lock (this.entityEventTypes)
            {
                this.entityEventTypes = new Dictionary<Type, Dictionary<string, Dictionary<string, HashSet<IViewModelEventHandler>>>>();
            }
        }

        public void RemoveAggregateRoot(string aggregateRootId)
        {
            foreach (var eventType in this.eventTypes)
                eventType.Value.Remove(aggregateRootId);

            foreach (var eventType in this.entityEventTypes)
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
