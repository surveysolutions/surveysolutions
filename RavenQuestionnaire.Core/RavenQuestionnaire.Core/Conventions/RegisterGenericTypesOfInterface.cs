using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Conventions.BindingGenerators;
using Ninject.Syntax;

namespace RavenQuestionnaire.Core.Conventions
{
    public class RegisterGenericTypesOfInterface : IBindingGenerator
    {
        private Type baseInterface;
        private Func<IContext, object> scope;
        public RegisterGenericTypesOfInterface(Type baseInterface)
        {
            this.baseInterface = baseInterface;
        }
        public RegisterGenericTypesOfInterface(Type baseInterface, Func<IContext, object> scope )
        {
            this.baseInterface = baseInterface;
            this.scope = scope;
        }
    /*    public void Process(Type type, Func<IContext, object> scopeCallback, IKernel kernel)
        {
            if (type.IsAbstract) { return; }
            if (type.IsInterface) { return; }
            var originalInterface = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == baseInterface);
            if (originalInterface == null) return;

            Type[] wrappedTypes = originalInterface.GetGenericArguments();

            // Create the created type
            Type implementationType = baseInterface.MakeGenericType(wrappedTypes);

            // And specify what we're going to use
            kernel.Bind(implementationType).To(type);
        }*/

        public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(Type targetType, IBindingRoot bindingRoot)
        {
            //  Assembly containingAssembly = type.Assembly;

            if (!baseInterface.IsGenericType)
                return Enumerable.Empty<IBindingWhenInNamedWithOrOnSyntax<object>>();
            var matchedTypes =
                targetType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == baseInterface);
            //   var result = new List<IBindingWhenInNamedWithOrOnSyntax<object>>();
            var retval = new List<IBindingWhenInNamedWithOrOnSyntax<object>>();
            foreach (Type matchedType in matchedTypes)
            {


                Type[] wrappedTypes = matchedType.GetGenericArguments();

                // Create the created type
                var genericInterface = baseInterface.MakeGenericType(wrappedTypes);
                retval.Add(bindingRoot.Bind(genericInterface).To(targetType));



                /*  var factoryInterface = type.MakeGenericType(originalInterface);
              //  var factory = typeof(RepositoryFactory<,>).MakeGenericType(type, originalInterface);

                // Bind the factory interface to the implementation
                result.Add(bindingRoot.Bind(factoryInterface).To(matchedGenericType));*/
            }
            return retval;


        }
    }
}
