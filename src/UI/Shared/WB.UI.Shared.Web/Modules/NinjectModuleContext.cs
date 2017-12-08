using System;
using Ninject;
using Ninject.Activation;
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
    }
}