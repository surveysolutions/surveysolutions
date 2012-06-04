using System;
using System.Linq;
using System.Reflection;
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
        }
    }
}
