using System;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using Ninject;
using Ninject.Activation;

namespace Main.Core.Utility
{
    public static class ReqistyHelper
    {
        public static void RegisterDenormalizer<T>(IKernel kernel)
        {
            Type[] eventHandlerTypes = { typeof(IEventHandler<>), typeof(IEventHandler) };
            var denormalizerType = typeof(T);
            Func<IContext, object> scope = (c) => kernel;

            foreach (var interfaceType in eventHandlerTypes)
            {
                kernel.Bind(interfaceType).To(denormalizerType).InScope(scope);

                if (!interfaceType.IsGenericType) continue;
                var interfaceImplementations = denormalizerType.GetInterfaces()
                    .Where(t => t.IsGenericType)
                    .Where(t => t.GetGenericTypeDefinition() == interfaceType);

                var genericInterfaceTypes = interfaceImplementations.Select(interfaceImplementation => interfaceType.MakeGenericType(interfaceImplementation.GetGenericArguments()));

                foreach (var genericInterfaceType in genericInterfaceTypes)
                {
                    kernel.Bind(genericInterfaceType).To(denormalizerType).InScope(scope);
                }
            }
        }
    }
}
