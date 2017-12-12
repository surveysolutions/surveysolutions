using System;
using Ninject;
using Ninject.Activation;
using Ninject.Syntax;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public class NinjectModuleContext : IModuleContext
    {
        private readonly IContext context;

        public NinjectModuleContext(IContext context)
        {
            this.context = context;
        }

        public T Resolve<T>()
        {
            return context.Kernel.Get<T>();
        }

        public Type MemberDeclaringType => context.Request.Target?.Member.DeclaringType;

        public T Inject<T>()
        {
            return context.Kernel.Get<T>();
        }

        public T Get<T>()
        {
            return context.Kernel.Get<T>();
        }

        public T Get<T>(string name)
        {
            return context.Kernel.Get<T>(name);
        }

        public object GetServiceWithGenericType(Type type, Type genericType)
        {
            return context.Kernel.GetService(type.MakeGenericType(genericType));
        }

        public Type GetGenericArgument()
        {
            return context.GenericArguments[0];
        }
    }

    public class NinjectConstructorContext : IConstructorContext
    {
        private readonly IContext context;

        public NinjectConstructorContext(IContext context)
        {
            this.context = context;
        }

        public T Inject<T>()
        {
            return context.Kernel.Get<T>();
        }
    }
}