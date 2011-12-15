using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;

namespace RavenQuestionnaire.Core.Conventions
{
    public class RegisterFirstInstanceOfInterface : IBindingGenerator
    {
        public void Process(Type type, Func<IContext, object> scopeCallback, IKernel kernel)
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
            var matchedType = containingAssembly.GetTypes()
                .Where(x =>
                       x.Namespace == type.Namespace
                       && x.GetInterface(type.FullName) != null)
                .FirstOrDefault();
            if (matchedType == null)
            {
                return;
            }
            kernel.Bind(type).To(matchedType);
          //  registry.For(type).Use(matchedType);
        }
    }
}
