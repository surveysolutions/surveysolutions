using System;
using Ncqrs.Eventing.Storage;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IIocRegistry
    {
        void Bind<TInterface, TImplementation>() where TImplementation : TInterface;
        void Bind<TImplementation>();
        void BindAsSingleton<TInterface, TImplementation>() where TImplementation : TInterface;
        void BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface;
        void BindToRegisteredInterface<TInterface, TRegisteredInterface>() where TRegisteredInterface : TInterface;
        void BindToMethod<T>(Func<T> func);
        void BindToConstant<T>(Func<T> func);
        void BindAsSingleton(Type @interface, Type implementation);
        void BindGeneric(Type implementation);
    }
}