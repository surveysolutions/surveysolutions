using System;
using System.Collections.Generic;
using MvvmCross.Platform.Core;
using MvvmCross.Platform.IoC;
using Ninject;
using Ninject.Modules;

namespace WB.UI.Shared.Enumerator.Ninject
{
    public class NinjectMvxIocProvider : MvxSingleton<IMvxIoCProvider>, IMvxIoCProvider
    {
        private readonly IKernel kernel;
        private readonly Dictionary<Type, List<Action>> pluginsForLazyRegistration = new Dictionary<Type, List<Action>>();

        public NinjectMvxIocProvider(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public NinjectMvxIocProvider(params INinjectModule[] modules)
            : this(new StandardKernel(modules)) {}

        public NinjectMvxIocProvider(INinjectSettings settings, params INinjectModule[] modules)
            : this(new StandardKernel(settings, modules)) {}

        public void CallbackWhenRegistered(Type type, Action action)
        {
            if (!this.CanResolve(type))
            {
                List<Action> actions;
                if (this.pluginsForLazyRegistration.TryGetValue(type, out actions))
                {
                    actions.Add(action);
                }
                else
                {
                    actions = new List<Action> {action};
                    this.pluginsForLazyRegistration[type] = actions;
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
            return (bool)this.kernel.CanResolve(type);
        }

        public bool CanResolve<T>() where T : class
        {
            return this.kernel.CanResolve<T>();
        }

        public object Create(Type type)
        {
            return this.kernel.Get(type);
        }

        public T Create<T>() where T : class
        {
            return this.kernel.Get<T>();
        }

        public object GetSingleton(Type type)
        {
            return this.kernel.Get(type);
        }

        public T GetSingleton<T>() where T : class
        {
            return this.kernel.Get<T>();
        }

        public object IoCConstruct(Type type)
        {
            return this.kernel.Get(type);
        }

        public T IoCConstruct<T>() where T : class
        {
            return this.kernel.Get<T>();
        }

        public void RegisterSingleton(Type tInterface, Func<object> theConstructor)
        {
            this.kernel.Bind(tInterface).ToMethod(context => theConstructor()).InSingletonScope();
        }

        public void RegisterSingleton(Type tInterface, object theObject)
        {
            this.kernel.Bind(tInterface).ToConstant(theObject).InSingletonScope();
        }

        public void RegisterSingleton<TInterface>(Func<TInterface> theConstructor) where TInterface : class
        {
            this.kernel.Bind<TInterface>().ToMethod(context => theConstructor()).InSingletonScope();
        }

        public void RegisterSingleton<TInterface>(TInterface theObject) where TInterface : class
        {
            this.kernel.Bind<TInterface>().ToConstant(theObject).InSingletonScope();
            this.LazyPluginsRegistration(typeof(TInterface));
        }

        public void RegisterType(Type tFrom, Type tTo)
        {
            this.kernel.Bind(tFrom).To(tTo);
        }

        public void RegisterType(Type t, Func<object> constructor)
        {
            this.kernel.Bind(t).ToMethod(context => constructor());
        }

        public void RegisterType<TInterface>(Func<TInterface> constructor) where TInterface : class
        {
            this.kernel.Bind<TInterface>().ToMethod(context => constructor());
        }

        public object Resolve(Type type)
        {
            return this.kernel.Get(type);
        }

        public T Resolve<T>() where T : class
        {
            return this.kernel.Get<T>();
        }

        public bool TryResolve(Type type, out object resolved)
        {
            resolved = this.kernel.TryGet(type);
            return (resolved != null);
        }

        public bool TryResolve<T>(out T resolved) where T : class
        {
            resolved = this.kernel.TryGet<T>();
            return (resolved != null);
        }

        void IMvxIoCProvider.RegisterType<TFrom, TTo>()
        {
            this.kernel.Unbind<TFrom>();
            this.kernel.Bind<TFrom>().To<TTo>();
        }

        private void LazyPluginsRegistration(Type tInterface)
        {
            List<Action> actions;
            lock (this)
            {
                if (this.pluginsForLazyRegistration.TryGetValue(tInterface, out actions))
                    this.pluginsForLazyRegistration.Remove(tInterface);
            }

            if (actions == null) return;

            foreach (var action in actions)
            {
                action();
            }
        }
    }
}