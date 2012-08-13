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
    public class RegisterFirstInstanceOfInterface : IBindingGenerator
    {
       /* public void Process(Type type, Func<IContext, object> scopeCallback, IKernel kernel)
        {
            if (!type.IsInterface)
            {
                return;
            }
            if (type.IsGenericTypeDefinition)
            {
                return;
            }

            Assembly containingAssembly = type.Assembly;
            var matchedType = containingAssembly.GetTypes().FirstOrDefault(x => x.Namespace == type.Namespace
                                                                                && x.GetInterface(type.FullName) != null);
            if (matchedType == null)
            {
                return;
            }
            if(kernel.TryGet(type)!=null)
                return;
            kernel.Bind(type).To(matchedType);
          //  registry.For(type).Use(matchedType);
        }*/

        public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(Type type, IBindingRoot bindingRoot)
        {
            IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> y =
                Enumerable.Empty<IBindingWhenInNamedWithOrOnSyntax<object>>();

            if (!type.IsInterface)
                return y;
            
            Assembly containingAssembly = type.Assembly;

            if (type.IsGenericType)
                return y;

            var matchedType =
                containingAssembly.GetTypes().FirstOrDefault(
                    x => x.Namespace == type.Namespace && !x.IsAbstract && x.GetInterface(type.FullName) != null);
            if (matchedType == null)
                return y;
            return
                new
                    IBindingWhenInNamedWithOrOnSyntax<object>[1] {bindingRoot.Bind(new Type[1] {type}).To(matchedType)};


        }
    }
}
