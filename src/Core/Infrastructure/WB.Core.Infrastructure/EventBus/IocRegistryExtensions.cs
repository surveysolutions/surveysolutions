using System;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure.EventBus
{
    public static class IocRegistryExtensions
    {
        public static void RegisterDenormalizer<T>(this IIocRegistry kernel) where T : IEventHandler
        {
            Type[] eventHandlerTypes = { typeof(IEventHandler), typeof(IEventHandler<>) };
            var denormalizerType = typeof(T);

            kernel.Bind(typeof(IEventHandler), denormalizerType);

            foreach (var interfaceType in eventHandlerTypes)
            {
                if (!interfaceType.IsGenericType) continue;

                RegisterEachHandledEventImplementation(kernel, denormalizerType, interfaceType);
            }
        }

        private static void RegisterEachHandledEventImplementation(IIocRegistry kernel, Type factoryType, Type interfaceType)
        {
            var interfaceImplementations = factoryType.GetInterfaces()
                .Where(t => t.IsGenericType)
                .Where(t => t.GetGenericTypeDefinition() == interfaceType);

            var genericInterfaceTypes =
                interfaceImplementations.Select(
                    interfaceImplementation => interfaceType.MakeGenericType(interfaceImplementation.GetGenericArguments()));

            foreach (var genericInterfaceType in genericInterfaceTypes)
            {
                kernel.Bind(genericInterfaceType, factoryType);
            }
        }
    }
}