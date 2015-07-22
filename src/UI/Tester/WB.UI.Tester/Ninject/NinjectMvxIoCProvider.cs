using System;
using System.Collections.Generic;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.IoC;
using Ninject;
using Ninject.Modules;

namespace WB.UI.Tester.Ninject
{
    public class NinjectMvxIocProvider : MvxSingleton<IMvxIoCProvider>, IMvxIoCProvider
    {
        private readonly StandardKernel kernel = new StandardKernel();
        private readonly Dictionary<Type, List<Action>> pluginsForLazyRegistration = new Dictionary<Type, List<Action>>();

        public NinjectMvxIocProvider(params INinjectModule[] modules)
        {
            kernel = new StandardKernel(modules);
        }

        public NinjectMvxIocProvider(INinjectSettings settings, params INinjectModule[] modules)
        {
            kernel = new StandardKernel(settings, modules);
        }

        public void CallbackWhenRegistered(Type type, Action action)
        {
            if (!CanResolve(type))
            {
                List<Action> actions;
                if (pluginsForLazyRegistration.TryGetValue(type, out actions))
                {
                    actions.Add(action);
                }
                else
                {
                    actions = new List<Action> {action};
                    pluginsForLazyRegistration[type] = actions;
                }
                return;
            }

            action();
        }

        public void CallbackWhenRegistered<T>(Action action)
        {
            this.CallbackWhenRegistered(typeof(T), action);
        }

        public bool CanResolve(Type type)
        {
            return (bool)kernel.CanResolve(type);
        }

        public bool CanResolve<T>() where T : class
        {
            return kernel.CanResolve<T>();
        }

        public object Create(Type type)
        {
            return kernel.Get(type);
        }

        public T Create<T>() where T : class
        {
            return kernel.Get<T>();
        }

        public object GetSingleton(Type type)
        {
            return kernel.Get(type);
        }

        public T GetSingleton<T>() where T : class
        {
            return kernel.Get<T>();
        }

        public object IoCConstruct(Type type)
        {
            return kernel.Get(type);
        }

        public T IoCConstruct<T>() where T : class
        {
            return kernel.Get<T>();
        }

        public void RegisterSingleton(Type tInterface, Func<object> theConstructor)
        {
            kernel.Bind(tInterface).ToMethod(context => theConstructor()).InSingletonScope();
        }

        public void RegisterSingleton(Type tInterface, object theObject)
        {
            kernel.Bind(tInterface).ToConstant(theObject).InSingletonScope();
        }

        public void RegisterSingleton<TInterface>(Func<TInterface> theConstructor) where TInterface : class
        {
            kernel.Bind<TInterface>().ToMethod(context => theConstructor()).InSingletonScope();
        }

        public void RegisterSingleton<TInterface>(TInterface theObject) where TInterface : class
        {
            kernel.Bind<TInterface>().ToConstant(theObject).InSingletonScope();
            LazyPluginsRegistration(typeof(TInterface));
        }

        public void RegisterType(Type tFrom, Type tTo)
        {
            kernel.Bind(tFrom).To(tTo);
        }

        public void RegisterType(Type t, Func<object> constructor)
        {
            kernel.Bind(t).ToMethod(context => constructor());
        }

        public void RegisterType<TInterface>(Func<TInterface> constructor) where TInterface : class
        {
            kernel.Bind<TInterface>().ToMethod(context => constructor());
        }

        public object Resolve(Type type)
        {
            return kernel.Get(type);
        }

        public T Resolve<T>() where T : class
        {
            return kernel.Get<T>();
        }

        public bool TryResolve(Type type, out object resolved)
        {
            resolved = kernel.TryGet(type);
            return (resolved != null);
        }

        public bool TryResolve<T>(out T resolved) where T : class
        {
            resolved = kernel.TryGet<T>();
            return (resolved != null);
        }

        void IMvxIoCProvider.RegisterType<TFrom, TTo>()
        {
            kernel.Unbind<TFrom>();
            kernel.Bind<TFrom>().To<TTo>();
        }

        private void LazyPluginsRegistration(Type tInterface)
        {
            List<Action> actions;
            lock (this)
            {
                if (pluginsForLazyRegistration.TryGetValue(tInterface, out actions))
                    pluginsForLazyRegistration.Remove(tInterface);
            }

            if (actions == null) return;

            foreach (var action in actions)
            {
                action();
            }
        }
    }
}