using System;
using Ninject;
using Ninject.Modules;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public class NinjectModuleAdapter<TModule> : NinjectModule, IIocRegistry
        where TModule : IModule
    {
        private readonly TModule module;

        public NinjectModuleAdapter(TModule module)
        {
            this.module = module;
        }

        public override void Load()
        {
            this.module.Load(this);
        }

        void IIocRegistry.Bind<TInterface, TImplementation>()
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>();
        }

        void IIocRegistry.BindAsSingleton<TInterface, TImplementation>()
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope();
        }

        void IIocRegistry.BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue)
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope()
                .WithConstructorArgument(argumentName, argumentValue);
        }

        void IIocRegistry.BindToRegisteredInterface<TInterface, TRegisteredInterface>()
        {
            this.Kernel.Bind<TInterface>().ToMethod<TInterface>(context => context.Kernel.Get<TRegisteredInterface>());
        }

        public void BindToMethod<T>(Func<T> func)
        {
            this.Kernel.Bind<T>().ToMethod(ctx => func());
        }

        public void BindToConstant<T>(Func<T> func)
        {
            this.Kernel.Bind<T>().ToMethod(ctx => func()).InSingletonScope();
        }

        public void BindAsSingleton(Type @interface, Type implementation)
        {
            throw new NotImplementedException();
        }
    }
}