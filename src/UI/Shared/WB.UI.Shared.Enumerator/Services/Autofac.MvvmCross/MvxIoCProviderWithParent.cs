using System;
using System.Collections.Generic;
using Autofac;
using MvvmCross.Base;
using MvvmCross.IoC;

namespace WB.UI.Shared.Enumerator.Services.Autofac.MvvmCross
{
      public class MvxIoCProviderWithParent : MvxSingleton<IMvxIoCProvider>, IMvxIoCProvider
    {
        private readonly MvxIoCContainer provider;

        public MvxIoCProviderWithParent (IMvxIocOptions options, IContainer container)
        {
            provider = new MvxIoCContainer(options, 
                new AutofacMvxIocProvider(container));
        }

        public bool CanResolve<T>()
            where T : class
        {
            return provider.CanResolve<T>();
        }

        public bool CanResolve(Type t)
        {
            return provider.CanResolve(t);
        }

        public bool TryResolve<T>(out T resolved)
            where T : class
        {
            return provider.TryResolve<T>(out resolved);
        }

        public bool TryResolve(Type type, out object resolved)
        {
            return provider.TryResolve(type, out resolved);
        }

        public T Resolve<T>()
            where T : class
        {
            return provider.Resolve<T>();
        }

        public object Resolve(Type t)
        {
            return provider.Resolve(t);
        }

        public T GetSingleton<T>()
            where T : class
        {
            return provider.GetSingleton<T>();
        }

        public object GetSingleton(Type t)
        {
            return provider.GetSingleton(t);
        }

        public T Create<T>()
            where T : class
        {
            return provider.Create<T>();
        }

        public object Create(Type t)
        {
            return provider.Create(t);
        }

        public void RegisterType<TInterface, TToConstruct>()
            where TInterface : class
            where TToConstruct : class, TInterface
        {
            provider.RegisterType<TInterface, TToConstruct>();
        }

        public void RegisterType<TInterface>(Func<TInterface> constructor)
            where TInterface : class
        {
            provider.RegisterType(constructor);
        }

        public void RegisterType(Type t, Func<object> constructor)
        {
            provider.RegisterType(t, constructor);
        }

        public void RegisterType(Type interfaceType, Type constructType)
        {
            provider.RegisterType(interfaceType, constructType);
        }

        public void RegisterSingleton<TInterface>(TInterface theObject)
            where TInterface : class
        {
            provider.RegisterSingleton(theObject);
        }

        public void RegisterSingleton(Type interfaceType, object theObject)
        {
            provider.RegisterSingleton(interfaceType, theObject);
        }

        public void RegisterSingleton<TInterface>(Func<TInterface> theConstructor)
            where TInterface : class
        {
            provider.RegisterSingleton(theConstructor);
        }

        public void RegisterSingleton(Type interfaceType, Func<object> theConstructor)
        {
            provider.RegisterSingleton(interfaceType, theConstructor);
        }

        public T IoCConstruct<T>()
            where T : class
        {
            return provider.IoCConstruct<T>();
        }

        public virtual object IoCConstruct(Type type)
        {
            return provider.IoCConstruct(type);
        }

        public T IoCConstruct<T>(IDictionary<string, object> arguments) where T : class
        {
            return provider.IoCConstruct<T>(arguments);
        }

        public T IoCConstruct<T>(params object[] arguments) where T : class
        {
            return provider.IoCConstruct<T>(arguments);
        }

        public T IoCConstruct<T>(object arguments) where T : class
        {
            return provider.IoCConstruct<T>(arguments);
        }

        public object IoCConstruct(Type type, IDictionary<string, object> arguments = null)
        {
            return provider.IoCConstruct(type, arguments);
        }

        public object IoCConstruct(Type type, object arguments)
        {
            return provider.IoCConstruct(type, arguments);
        }

        public object IoCConstruct(Type type, params object[] arguments)
        {
            return provider.IoCConstruct(type, arguments);
        }

        public void CallbackWhenRegistered<T>(Action action)
        {
            provider.CallbackWhenRegistered<T>(action);
        }

        public void CallbackWhenRegistered(Type type, Action action)
        {
            provider.CallbackWhenRegistered(type, action);
        }

        public void CleanAllResolvers()
        {
            provider.CleanAllResolvers();
        }

        public IMvxIoCProvider CreateChildContainer()
        {
            return provider.CreateChildContainer();
        }
    }
}
