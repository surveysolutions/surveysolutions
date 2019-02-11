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
                                (typeof(IEventHandler<>) == i.GetGenericTypeDefinition() ||
                                 typeof(IAsyncEventHandler<>) == i.GetGenericTypeDefinition()))
                    .ToArray();

                MethodInfo GetHandler(Type eventHandlerInterface)
                {
                    return eventHandlerInterface.GetMethods()
                        .Single(m => m.GetCustomAttribute<HandlerAttribute>() != null);
                }

                try
                {
                    return interfaces.ToDictionary(i => i.GenericTypeArguments[0], GetHandler);
                }
                catch (ArgumentException)
                {
                    var errors = from @interface in interfaces
                        group @interface by @interface.GenericTypeArguments[0]
                        into errs
                        where errs.Count() > 1
                        select errs.Key;

                    var message =$"{handler.GetType().Name} handler register same interview event with async and sync handlers:";
                    foreach (var error in errors)
                    {
                        message += $"\r\n - '{error.Name}'";
                    }

                    throw new ArgumentException(message);
                }
            });
        }

        public static async Task Handle(this IStatefulDenormalizer denormalizer, Event ev, CancellationToken token = default)
        {
            var handler = denormalizer.GetEventHandlersMap();

            if (handler.TryGetValue(ev.Payload.GetType(), out var method))
            {
                if (method.ReturnType == typeof(Task))
                {
                    await (Task)method.Invoke(denormalizer, new[]
                    {
                        ev.AsPublishedEvent(), token
                    });
                }
                else
                {
                    method.Invoke(denormalizer, new[] { ev.AsPublishedEvent() });
                }
            }
        }
    }
}
