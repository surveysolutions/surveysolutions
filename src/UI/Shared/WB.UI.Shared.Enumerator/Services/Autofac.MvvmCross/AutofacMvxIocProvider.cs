using System;
using System.Collections.Generic;
using Autofac;
using MvvmCross.IoC;

namespace WB.UI.Shared.Enumerator.Services.Autofac.MvvmCross
{
    public class AutofacMvxIocProvider : IMvxIoCProvider
    {
        private readonly IContainer container;

        public AutofacMvxIocProvider(IContainer container)
        {
            this.container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public bool CanResolve<T>() where T : class
        {
            return container.IsRegistered<T>();
        }

        public bool CanResolve(Type type)
        {
            return container.IsRegistered(type);
        }

        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            return container.Resolve(type);
        }

        public T Create<T>() where T : class
        {
            return Resolve<T>();
        }

        public object Create(Type type)
        {
            return Resolve(type);
        }

        public T GetSingleton<T>() where T : class
        {
            return Resolve<T>();
        }

        public object GetSingleton(Type type)
        {
            return Resolve(type);
        }

        public bool TryResolve<T>(out T resolved) where T : class
        {
            return container.TryResolve(out resolved);
        }

        public bool TryResolve(Type type, out object resolved)
        {
            return container.TryResolve(type, out resolved);
        }
        
        public void RegisterType<TFrom, TTo>()
            where TFrom : class
            where TTo : class, TFrom
        {
            throw new NotImplementedException();
        }

        public void RegisterType<TInterface>(Func<TInterface> constructor) where TInterface : class
        {
            throw new NotImplementedException();
        }

        public void RegisterType(Type t, Func<object> constructor)
        {
            throw new NotImplementedException();
        }

        public void RegisterType(Type tFrom, Type tTo)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton<TInterface>(TInterface theObject) where TInterface : class
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton(Type tInterface, object theObject)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton<TInterface>(Func<TInterface> theConstructor) where TInterface : class
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton(Type tInterface, Func<object> theConstructor)
        {
            throw new NotImplementedException();
        }

        public T IoCConstruct<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public T IoCConstruct<T>(IDictionary<string, object> arguments) where T : class
        {
            throw new NotImplementedException();
        }

        public T IoCConstruct<T>(object arguments) where T : class
        {
            throw new NotImplementedException();
        }

        public T IoCConstruct<T>(params object[] arguments) where T : class
        {
            throw new NotImplementedException();
        }

        public object IoCConstruct(Type type)
        {
            throw new NotImplementedException();
        }

        public object IoCConstruct(Type type, IDictionary<string, object> arguments)
        {
            throw new NotImplementedException();
        }

        public object IoCConstruct(Type type, object arguments)
        {
            throw new NotImplementedException();
        }

        public object IoCConstruct(Type type, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public void CallbackWhenRegistered<T>(Action action)
        {
            throw new NotImplementedException();
        }

        public void CallbackWhenRegistered(Type type, Action action)
        {
            throw new NotImplementedException();
        }

        public IMvxIoCProvider CreateChildContainer()
        {
            throw new NotImplementedException();
        }
    }
}
