using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Infrastructure.EventSourcing
{
    public static class FunctionalHandlerExtensions
    {
        static readonly ConcurrentDictionary<Type, Dictionary<Type, MethodInfo>> eventsMapCache = new ConcurrentDictionary<Type, Dictionary<Type, MethodInfo>>();

        public static Dictionary<Type, MethodInfo> GetEventHandlersMap(this IStatefulDenormalizer handler)
        {
            return eventsMapCache.GetOrAdd(handler.GetType(), h =>
            {
                var interfaces = h.GetInterfaces()
                    .Where(i => i.GenericTypeArguments.Length == 1 &&
                                typeof(IEventHandler<>) == i.GetGenericTypeDefinition())
                    .ToArray();

                MethodInfo GetHandler(Type eventHandlerInterface)
                {
                    return eventHandlerInterface.GetMethods()
                        .Single(m => m.GetCustomAttribute<HandlerAttribute>() != null);
                }

                return interfaces.ToDictionary(i => i.GenericTypeArguments[0], GetHandler);
            });
        }

        public static void Handle(this IStatefulDenormalizer denormalizer, Event ev)
        {
            var handler = denormalizer.GetEventHandlersMap();

            if (handler.TryGetValue(ev.Payload.GetType(), out var method))
            {
                method.Invoke(denormalizer, new[]
                {
                    (object) ev.AsPublishedEvent()
                });
            }
        }
    }
}
