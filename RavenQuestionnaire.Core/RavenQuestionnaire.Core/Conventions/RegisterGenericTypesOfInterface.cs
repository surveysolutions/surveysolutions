using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;

namespace RavenQuestionnaire.Core.Conventions
{
    public class RegisterGenericTypesOfInterface : IBindingGenerator
    {
        private Type baseInterface;

        public RegisterGenericTypesOfInterface(Type baseInterface)
        {
            this.baseInterface = baseInterface;
        }
        
        public void Process(Type type, Func<IContext, object> scopeCallback, IKernel kernel)
        {
            if (type.IsAbstract) { return; }
            if (type.IsInterface) { return; }
            var originalInterface = type.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == baseInterface).FirstOrDefault();
            if (originalInterface == null) return;

            Type[] wrappedTypes = originalInterface.GetGenericArguments();

            // Create the created type
            Type implementationType = baseInterface.MakeGenericType(wrappedTypes);

            // And specify what we're going to use
            kernel.Bind(implementationType).To(type);
        }
    }
}
