using System;
using System.Linq;
using Main.Core.View;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject;
using Ninject.Activation;

namespace Main.Core.Utility
{
    public static class RegistryHelper
    {
        public static void RegisterFactory<T>(IKernel kernel)
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
