using System;

namespace WB.Core.Infrastructure.DependencyInjection
{
    public interface IDependencyRegistry
    {
        void Bind<TInterface, TImplementation>() where TImplementation : class, TInterface where TInterface : class;
        void Bind(Type @interface, Type implementation);
        void BindAsSingleton<TInterface, TImplementation>() where TImplementation : class, TInterface where TInterface : class;
        void BindAsSingleton<TInterface, TImplementation>(TImplementation instance) where TImplementation : class, TInterface where TInterface : class;
        void BindAsScoped<TInterface, TImplementation>() where TImplementation : class, TInterface where TInterface : class;
    }
}
