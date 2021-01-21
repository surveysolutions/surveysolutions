using System;
using Microsoft.Extensions.Configuration;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IIocRegistry
    {
        void Bind<TInterface, TImplementation>() where TImplementation : TInterface;
        void Bind(Type @interface, Type implementation);
        void Bind<TInterface1, TInterface2, TImplementation>() where TImplementation : TInterface1, TInterface2;
        void Bind<TInterface, TImplementation>(params ConstructorArgument[] constructorArguments) where TImplementation : TInterface;
        void Bind<TImplementation>(bool propertiesAutowired = false);
        void BindWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface;
        void BindWithConstructorArgument<TInterface1, TInterface2, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface1, TInterface2;
        void BindAsSingleton<TInterface, TImplementation>() where TImplementation : TInterface;
        void BindAsSingleton<TInterface1, TInterface2, TImplementation>() where TImplementation : TInterface2, TInterface1;
        void BindAsSingleton<TInterface1, TInterface2, TInterface3, TImplementation>() where TImplementation : TInterface3, TInterface2, TInterface1;
        void BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface;
        void BindAsSingletonWithConstructorArguments<TInterface, TImplementation>(params ConstructorArgument[] constructorArguments) where TImplementation : TInterface;
        void BindToRegisteredInterface<TInterface, TRegisteredInterface>() where TRegisteredInterface : TInterface;
        void BindToMethod<T>(Func<T> func, string name = null);
        void BindToMethod<T>(Func<IModuleContext, T> func, string name = null, bool externallyOwned = false);
        void BindToMethodInSingletonScope<T>(Func<IModuleContext, T> func, string named = null);
        void BindToConstant<T>(Func<T> func);
        void BindToConstant<T>(Func<IModuleContext, T> func);
        void BindAsSingleton(Type @interface, Type implementation);
        void BindAsSingleton(Type @interface, Type @interface2, Type implementation);
        void BindGeneric(Type implementation);
        void BindInPerLifetimeScope<T1, T2>() where T2 : T1;

        void BindInPerLifetimeScope<TInterface1, TInterface2, TImplementation>() where TImplementation : TInterface2, TInterface1;
    }
}
