using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject;
using Ninject.Activation;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public static class DispatcherRegistryHelper
    {
        public static void RegisterDenormalizer<T>(IKernel kernel)
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
