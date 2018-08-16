using System;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IIocRegistry
    {
        void Bind<TInterface, TImplementation>() where TImplementation : TInterface;
        void Bind(Type @interface, Type implementation);
        void Bind<TInterface1, TInterface2, TImplementation>() where TImplementation : TInterface1, TInterface2;
        void Bind<TInterface, TImplementation>(params ConstructorArgument[] constructorArguments) where TImplementation : TInterface;
        void Bind<TImplementation>();
        void BindWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface;
        void BindAsSingleton<TInterface, TImplementation>() where TImplementation : TInterface;
        void BindAsSingleton<TInterface1,TInterface2, TImplementation>() where TImplementation : TInterface1, TInterface2;
        void BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface;
        void BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(params ConstructorArgument[] constructorArguments) where TImplementation : TInterface;
        void BindToRegisteredInterface<TInterface, TRegisteredInterface>() where TRegisteredInterface : TInterface;
        void BindToMethod<T>(Func<T> func, string name = null);
        void BindToMethod<T>(Func<IModuleContext, T> func, string name = null);
        void BindToMethodInSingletonScope<T>(Func<IModuleContext, T> func, string named = null);
        void BindToMethodInSingletonScope(Type @interface, Func<IModuleContext, object> func);
        void BindToMethodInRequestScope<T>(Func<IModuleContext, T> func);
        void BindToConstant<T>(Func<T> func);
        void BindToConstant<T>(Func<IModuleContext, T> func);
        void BindToConstructorInSingletonScope<T>(Func<IConstructorContext, T> func);
        void BindAsSingleton(Type @interface, Type implementation);
        void BindGeneric(Type implementation);
        void Unbind<T>();
        bool HasBinding<T>();
        void BindInIsolatedThreadScopeOrRequestScopeOrThreadScope<T>();
        void BindInIsolatedThreadScopeOrRequestScopeOrThreadScope<T1, T2>() where T2 : T1;
    }
}
