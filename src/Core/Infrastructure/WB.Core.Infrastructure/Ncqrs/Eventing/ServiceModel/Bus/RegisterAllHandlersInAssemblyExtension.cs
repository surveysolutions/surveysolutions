using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public static class RegisterAllHandlersInAssemblyExtension
    {
        public static void RegisterHandler(this InProcessEventBus target, object handler, Type eventDataType)
        {
            var targetMethod = registerHandlerTargetMethodCache.GetOrAdd(eventDataType,
                type =>
                {
                    var registerHandlerMethod = target.GetType().GetTypeInfo().GetMethods().Single
                    (
                        m => m.Name == "RegisterHandler" && m.IsGenericMethod && m.GetParameters().Count() == 1
                    );

                    return registerHandlerMethod.MakeGenericMethod(new[] { eventDataType });
                });

            targetMethod.Invoke(target, new[] { handler });
        }

        private static readonly ConcurrentDictionary<Type, MethodInfo> registerHandlerTargetMethodCache = new ConcurrentDictionary<Type, MethodInfo>();
    }
}
