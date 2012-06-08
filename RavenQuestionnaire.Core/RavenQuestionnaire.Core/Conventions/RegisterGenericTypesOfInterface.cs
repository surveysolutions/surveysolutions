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

        public RegisterGenericTypesOfInterface(Type baseInterface)
        {
            this.baseInterface = baseInterface;
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
            var matchedType =
                targetType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == baseInterface);
         //   var result = new List<IBindingWhenInNamedWithOrOnSyntax<object>>();
            if(matchedType!=null)
            {
                Type[] wrappedTypes = matchedType.GetGenericArguments();

                // Create the created type
                var genericInterface = baseInterface.MakeGenericType(wrappedTypes);

                return new IBindingWhenInNamedWithOrOnSyntax<object>[1]
                           {bindingRoot.Bind(genericInterface).To(targetType)};


                /*  var factoryInterface = type.MakeGenericType(originalInterface);
              //  var factory = typeof(RepositoryFactory<,>).MakeGenericType(type, originalInterface);

                // Bind the factory interface to the implementation
                result.Add(bindingRoot.Bind(factoryInterface).To(matchedGenericType));*/
            }
            return  Enumerable.Empty<IBindingWhenInNamedWithOrOnSyntax<object>>();
         

        }
    }
}
