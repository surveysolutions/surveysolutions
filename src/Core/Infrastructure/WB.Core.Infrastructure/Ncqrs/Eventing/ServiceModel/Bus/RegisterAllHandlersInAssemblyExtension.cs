﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.Services;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public static class RegisterAllHandlersInAssemblyExtension
    {
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


        public static void RegisterHandler(this InProcessEventBus target, object handler, Type eventDataType)
        {
            var targetMethod = _registerHandlerTargetMethodCache.GetOrAdd(eventDataType,
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

        private static readonly ConcurrentDictionary<Type, MethodInfo> _registerHandlerTargetMethodCache = new ConcurrentDictionary<Type, MethodInfo>();

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
                   typeInfo.GetGenericTypeDefinition() == typeof(IEventHandler<>);
        }
    }
}
