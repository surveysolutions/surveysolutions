using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Activation;

namespace Main.Core.Utility
{
    public static class ReqistyHelper
    {
        public static void RegisterDemormalizer(IKernel kernel, Type[] interfaceTypes, Type implementation)
        {
            Func<IContext, object> scope = (c) => kernel;
            foreach (var interfaceType in interfaceTypes)
            {
                kernel.Bind(interfaceType).To(implementation).InScope(scope);
                if (interfaceType.IsGenericType)
                {
                    var interfaceImplementations = implementation.GetInterfaces()
                        .Where(t => t.IsGenericType)
                        .Where(t => t.GetGenericTypeDefinition() == interfaceType);

                    foreach (Type interfaceImplementation in interfaceImplementations)
                    {
                        Type genericInterfaceType = interfaceType.MakeGenericType(interfaceImplementation.GetGenericArguments());
                        kernel.Bind(genericInterfaceType)
                            .To(implementation)
                            .InScope(scope);
                    }
                }
            }
        }
    }
}
