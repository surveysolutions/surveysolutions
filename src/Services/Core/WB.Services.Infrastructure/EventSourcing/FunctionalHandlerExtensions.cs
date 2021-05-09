using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WB.ServicesIntegration.Export;

namespace WB.Services.Infrastructure.EventSourcing
{
    public static class FunctionalHandlerExtensions
    {
        static readonly ConcurrentDictionary<Type, Dictionary<Type, MethodInfo>> eventsMapCache = new ConcurrentDictionary<Type, Dictionary<Type, MethodInfo>>();

        private static Dictionary<Type, MethodInfo> GetEventHandlersMap(this IStatefulDenormalizer handler)
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
                catch (ArgumentException ae)
                {
                    var errors = from @interface in interfaces
                        group @interface by @interface.GenericTypeArguments[0]
                        into errs
                        where errs.Count() > 1
                        select errs.Key;

                    var message =$"{h.Name} handler register same interview event with async and sync handlers:";
                    foreach (var error in errors)
                    {
                        message += $"\r\n - '{error.Name}'";
                    }

                    throw new ArgumentException(message, ae);
                }
            });
        }

        public static async Task Handle(this IStatefulDenormalizer eventsHandler, Event ev, CancellationToken token = default)
        {
            if (eventsHandler == null) { throw new ArgumentNullException(nameof(eventsHandler)); }
            if (ev == null) { throw new ArgumentNullException(nameof(ev)); }

            var handler = eventsHandler.GetEventHandlersMap();
            if (handler == null) { throw new ArgumentNullException(nameof(handler)); }
            if (ev.Payload == null) { throw new ArgumentNullException(nameof(ev.Payload)); }

            if (handler.TryGetValue(ev.Payload.GetType(), out var method))
            {
                try
                {
                    if (method.ReturnType == typeof(Task))
                    {
                        await (Task) method.Invoke(eventsHandler, new[]
                        {
                            ev.AsPublishedEvent(), token
                        });
                    }
                    else
                    {
                        method.Invoke(eventsHandler, new[] {ev.AsPublishedEvent()});
                    }
                }
                catch (TargetInvocationException tie)
                {
                    var exception = tie.InnerException ?? tie;

                    exception.Data.Add("WB:handlerMethod",
                            $"{eventsHandler.GetType().Name}.{method.Name}<{ev.Payload.GetType().Name}>(...)");

                    exception.Data.Add("WB:initialException", tie.ToString());//saves original call stack and also inner exception info

                    throw exception;
                }
            }
        }
    }
}
