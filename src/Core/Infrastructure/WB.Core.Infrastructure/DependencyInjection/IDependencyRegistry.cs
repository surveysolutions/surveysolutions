using System;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure.DependencyInjection
{
    public interface IDependencyRegistry
    {
        void Bind<TInterface, TImplementation>() where TImplementation : class, TInterface where TInterface : class;
        void Bind(Type @interface, Type implementation);
        void BindAsSingleton<TInterface, TImplementation>() where TImplementation : class, TInterface where TInterface : class;
        void BindAsSingleton<TInterface, TImplementation>(TImplementation instance) where TImplementation : class, TInterface where TInterface : class;
        void BindAsScoped<TInterface, TImplementation>() where TImplementation : class, TInterface where TInterface : class;

        void BindToConstant<TInterface>(Func<IServiceProvider, TInterface> implementation) where TInterface : class;
        void BindToConstant<TInterface>(Func<TInterface> implementation) where TInterface : class;
        
        void BindAsSingleton(Type @interface, Type implementation);
        void BindToMethod<TImplementation>(Func<TImplementation> func, string name = null) where TImplementation : class;
        void BindToMethod<TImplementation>(Func<IServiceProvider, TImplementation> func, string name = null) where TImplementation : class;
        void BindToMethodInSingletonScope<T>(Func<IServiceProvider, T> func, string named = null);
        void RegisterDenormalizer<T>() where T : IEventHandler;
    }
}
