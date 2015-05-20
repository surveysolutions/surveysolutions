using System;
using System.Linq;
using System.Reflection;
using WB.Core.GenericSubdomains.Portable.Services;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public static class RegisterAllHandlersInAssemblyExtension
    {
        private static ILogger _log = LogManager.GetLogger(typeof(RegisterAllHandlersInAssemblyExtension));

        public static void RegisterAllHandlersInAssembly(this InProcessEventBus target, Assembly asm)
        {
            target.RegisterAllHandlersInAssembly(asm, CreateInstance);
        }

        public static void RegisterAllHandlersInAssembly(this InProcessEventBus target, Assembly asm, Func<Type, object> handlerFactory)
        {
            foreach (var typeInfo in asm.DefinedTypes.Where(ImplementsAtLeastOneIEventHandlerInterface))
            {
                var handler = handlerFactory(typeInfo.DeclaringType);

                foreach (var handlerInterfaceType in typeInfo.ImplementedInterfaces.Where(IsIEventHandlerInterface))
                {
                    var eventDataType = handlerInterfaceType.GenericTypeArguments.First();
                    target.RegisterHandler(handler, eventDataType);
                }
            }
        }

        private static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public static void RegisterHandler(this InProcessEventBus target,object handler, Type eventDataType)
        {
            NcqrsEnvironment.RegisterEventDataType(eventDataType);

            var registerHandlerMethod = target.GetType().GetMethods().Single
            (
                m => m.Name == "RegisterHandler" && m.IsGenericMethod && m.GetParameters().Count() == 1
            );

            var targetMethod = registerHandlerMethod.MakeGenericMethod(new[] { eventDataType });
            targetMethod.Invoke(target, new object[] { handler });

            _log.InfoFormat("Registered {0} as event handler for event {1}.", handler.GetType().FullName, eventDataType.FullName);
        }

        private static bool ImplementsAtLeastOneIEventHandlerInterface(TypeInfo typeInfo)
        {
            return typeInfo.IsClass && !typeInfo.IsAbstract &&
                   typeInfo.ImplementedInterfaces.Any(IsIEventHandlerInterface);
        }

        private static bool IsIEventHandlerInterface(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsInterface &&
                   typeInfo.IsGenericType &&
                   typeInfo.GetGenericTypeDefinition() == typeof (IEventHandler<>);
        }
    }
}
