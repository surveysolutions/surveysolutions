using System;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IIocRegistry
    {
        void Bind<TInterface, TImplementation>() where TImplementation : TInterface;
        void Bind<TInterface1, TInterface2, TImplementation>() where TImplementation : TInterface1, TInterface2;
        void Bind<TInterface, TImplementation>(params ConstructorArgument[] constructorArguments) where TImplementation : TInterface;
        void Bind<TImplementation>();
        void BindAsSingleton<TInterface, TImplementation>() where TImplementation : TInterface;
        void BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface;
        void BindToRegisteredInterface<TInterface, TRegisteredInterface>() where TRegisteredInterface : TInterface;
        void BindToMethod<T>(Func<T> func);
        void BindToConstant<T>(Func<T> func);
        void BindToConstructor<T>(Func<IConstructorContext, T> func);
        void BindAsSingleton(Type @interface, Type implementation);
        void BindGeneric(Type implementation);
        void RegisterDenormalizer<T>() where T : IEventHandler;
    }
}