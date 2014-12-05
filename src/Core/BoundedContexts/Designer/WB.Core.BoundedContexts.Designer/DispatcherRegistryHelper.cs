using System;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject;
using Ninject.Activation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Designer
{
    public static class DispatcherRegistryHelper
    {
        public static void RegisterDenormalizer<T>(this IKernel kernel) where T : IEventHandler
        {
            Type[] eventHandlerTypes = { typeof (IEventHandler), typeof (IEventHandler<>) };
            var denormalizerType = typeof (T);
            Func<IContext, object> scope = (c) => kernel;

            foreach (var interfaceType in eventHandlerTypes)
            {
                kernel.Bind(interfaceType).To(denormalizerType).InScope(scope);

                if (!interfaceType.IsGenericType) continue;

                GetValue(kernel, denormalizerType, interfaceType, scope);
            }
        }

        public static void RegisterFactory<T>(this IKernel kernel)
        {
            var interfaceType = typeof(IViewFactory<,>);
            var factoryType = typeof(T);
            Func<IContext, object> scope = (c) => kernel;

            GetValue(kernel, factoryType, interfaceType, scope);
        }

        private static void GetValue(IKernel kernel, Type factoryType, Type interfaceType, Func<IContext, object> scope)
        {
            var interfaceImplementations = factoryType.GetInterfaces()
                .Where(t => t.IsGenericType)
                .Where(t => t.GetGenericTypeDefinition() == interfaceType);

            var genericInterfaceTypes =
                interfaceImplementations.Select(
                    interfaceImplementation => interfaceType.MakeGenericType(interfaceImplementation.GetGenericArguments()));

            foreach (var genericInterfaceType in genericInterfaceTypes)
            {
                kernel.Bind(genericInterfaceType).To(factoryType).InScope(scope);
            }
        }
    }
}
