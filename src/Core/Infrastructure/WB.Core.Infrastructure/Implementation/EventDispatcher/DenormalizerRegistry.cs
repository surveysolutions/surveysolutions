using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Ncqrs.Eventing;

namespace WB.Core.Infrastructure.Implementation.EventDispatcher
{
    public class DenormalizerRegistry : IDenormalizerRegistry
    {
        private readonly EventBusSettings eventBusSettings;
        private readonly List<Type> batchProcessingDenormalizers = new List<Type>();

        private readonly Dictionary<Type, List<EventHandlerMethod>> sequentialDenormalizers = new Dictionary<Type, List<EventHandlerMethod>>();

        public DenormalizerRegistry(EventBusSettings eventBusSettings)
        {
            this.eventBusSettings = eventBusSettings;
        }

        public void RegisterFunctional<T>() where T : IFunctionalEventHandler
        {
            if(eventBusSettings.DisabledEventHandlerTypes.Any(d => d == typeof(T)))
            {
                return;
            }

            batchProcessingDenormalizers.Add(typeof(T));
        }

        public void Register<T>() where T : IEventHandler
        {
            var denormalizerType = typeof(T);
            var receivesIgnoredEventsAttribute = denormalizerType.GetCustomAttribute(typeof(ReceivesIgnoredEventsAttribute));

            var handlers = new List<EventHandlerMethod>();

            foreach (var @interface in denormalizerType.GetInterfaces())
            {
                if (!@interface.IsGenericType || @interface.GetGenericTypeDefinition() != typeof(IEventHandler<>))
                    continue;

                Type handledEventType = @interface.GetGenericArguments()[0];


                MethodInfo handlerMethod = null;
                foreach (var methodInfo in denormalizerType.GetMethods())
                {
                    var parameterInfos = methodInfo.GetParameters();
                    if (methodInfo.Name == "Handle" && parameterInfos.Length == 1)
                    {
                        var parameterType = parameterInfos[0].ParameterType;
                        if (parameterType.IsGenericType)
                        {
                            var genericTypeArguments = parameterType.GetGenericArguments();
                            if (genericTypeArguments.Length == 1 && genericTypeArguments[0] == handledEventType)
                            {
                                handlerMethod = methodInfo;
                                break;
                            }
                        }
                    }
                }

                handlers.Add(new EventHandlerMethod
                {
                    EventType = handledEventType,
                    ReceivesIgnoredEvents = receivesIgnoredEventsAttribute != null,
                    Handle = handlerMethod
                });
            }

            sequentialDenormalizers[denormalizerType] = handlers;
        }

        public IReadOnlyCollection<Type> FunctionalDenormalizers => batchProcessingDenormalizers;

        public IReadOnlyCollection<Type> SequentialDenormalizers => sequentialDenormalizers.Keys;

        public EventHandlerMethod HandlerMethod(Type denormalizer, Type eventType) =>
            sequentialDenormalizers[denormalizer].First(x => x.EventType == eventType);

        public bool CanHandleEvent(Type denormalizerType, IPublishableEvent evnt)
        {
            if (!sequentialDenormalizers.ContainsKey(denormalizerType))
                throw new InvalidOperationException($"Denormalizer {denormalizerType.Name} is not registered, call {nameof(Register)} method.");

            var payloadType = evnt.Payload.GetType();

            return sequentialDenormalizers[denormalizerType].Any(x => x.EventType == payloadType);
        }
    }
}
