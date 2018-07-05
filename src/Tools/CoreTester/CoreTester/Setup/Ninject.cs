using System;
using System.Linq;
using System.Threading;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Ninject.Syntax;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Threading;

namespace CoreTester.Setup
{
    public class NinjectModuleAdapter<TModule> : NinjectModuleAdapter
        where TModule : IModule
    {
        private readonly TModule module;

        public NinjectModuleAdapter(TModule module)
        {
            this.module = module;
        }

        public override void Load()
        {
            this.module.Load(this);
        }

        public override string Name => this.module.ToString();
    }

    public abstract class NinjectModuleAdapter : NinjectModule, IIocRegistry
    {
        void IIocRegistry.Bind<TInterface, TImplementation>()
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>();
        }

        public void Bind(Type @interface, Type implementation)
        {
            this.Kernel.Bind(@interface).To(implementation);
        }

        void IIocRegistry.Bind<TInterface1, TInterface2, TImplementation>() 
        {
            this.Kernel.Bind<TInterface1, TInterface2>().To<TImplementation>();
        }

        void IIocRegistry.Bind<TInterface, TImplementation>(params ConstructorArgument[] constructorArguments)
        {
            var syntax = this.Kernel.Bind<TInterface>().To<TImplementation>();

            foreach (var constructorArgument in constructorArguments)
            {
                object Callback(IContext context) => constructorArgument.Value.Invoke(new NinjectModuleContext(context));
                syntax.WithConstructorArgument(constructorArgument.Name, (Func<IContext, object>) Callback);
            }
        }

        void IIocRegistry.Bind<TImplementation>()
        {
            this.Kernel.Bind<TImplementation>().ToSelf();
        }

        public void BindWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>().WithConstructorArgument(argumentName, argumentValue);
        }

        void IIocRegistry.BindAsSingleton<TInterface, TImplementation>()
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope();
        }

        void IIocRegistry.BindAsSingleton<TInterface1, TInterface2, TImplementation>()
        {
            this.Kernel.Bind<TInterface1, TInterface2>().To<TImplementation>().InSingletonScope();
        }

        void IIocRegistry.BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue)
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope()
                .WithConstructorArgument(argumentName, argumentValue);
        }

        public void BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(
            params ConstructorArgument[] constructorArguments) where TImplementation : TInterface
        {
            var syntax = this.Kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope();
            foreach (var constructorArgument in constructorArguments)
            {
                syntax.WithConstructorArgument(constructorArgument.Name, c => constructorArgument.Value(new NinjectModuleContext(c)));
            }
        }

        void IIocRegistry.BindToRegisteredInterface<TInterface, TRegisteredInterface>()
        {
            this.Kernel.Bind<TInterface>().ToMethod<TInterface>(context => context.Kernel.Get<TRegisteredInterface>());
        }

        void IIocRegistry.BindToMethod<T>(Func<T> func, string name)
        {
            var syntax = this.Kernel.Bind<T>().ToMethod(ctx => func());
            if (!string.IsNullOrEmpty(name))
                syntax.Named(name);
        }

        public void BindToMethod<T>(Func<IModuleContext, T> func, string name)
        {
            var syntax = this.Kernel.Bind<T>().ToMethod(ctx => func(new NinjectModuleContext(ctx)));
            if (!string.IsNullOrEmpty(name))
                syntax.Named(name);
        }

        public void BindToMethodInSingletonScope<T>(Func<IModuleContext, T> func, string named = null)
        {
            var syntax = this.Kernel.Bind<T>().ToMethod(c => func(new NinjectModuleContext(c))).InSingletonScope();
            if (!string.IsNullOrEmpty(named))
                syntax.Named(named);
        }

        public void BindToMethodInSingletonScope(Type @interface, Func<IModuleContext, object> func)
        {
            this.Kernel.Bind(@interface).ToMethod(c => func(new NinjectModuleContext(c))).InSingletonScope();
        }

        public void BindToMethodInRequestScope<T>(Func<IModuleContext, T> func)
        {
            this.Kernel.Bind<T>().ToMethod(ctx => func(new NinjectModuleContext(ctx))).InSingletonScope();
        }

        void IIocRegistry.BindToConstant<T>(Func<T> func)
        {
            this.Kernel.Bind<T>().ToMethod(ctx => func()).InSingletonScope();
        }

        void IIocRegistry.BindToConstant<T>(Func<IModuleContext, T> func)
        {
            this.Kernel.Bind<T>().ToMethod(ctx => func(new NinjectModuleContext(ctx))).InSingletonScope();
        }

        public void BindToConstructorInSingletonScope<T>(Func<IConstructorContext, T> func)
        //public void BindToConstructorInSingletonScope<T>(Func<IConstructorContext, T> func)
        {
            //this.Kernel.Bind<T>().ToConstructor(ctx => func(new NinjectConstructorContext(ctx))).InSingletonScope();
            this.Kernel.Bind<T>().ToMethod(ctx => func(new NinjectConstructorContext(ctx))).InSingletonScope();
        }

        void IIocRegistry.BindAsSingleton(Type @interface, Type implementation)
        {
            this.Kernel.Bind(@interface).To(implementation).InSingletonScope();
        }

        void IIocRegistry.BindGeneric(Type implementation)
        {
            this.Kernel.Bind(implementation);
        }

        void IIocRegistry.Unbind<TInterface>()
        {
            this.Kernel.Unbind<TInterface>();
        }

        public bool HasBinding<T>()
        {
            return this.Kernel.GetBindings(typeof(T)).Any();
        }

        public void BindInIsolatedThreadScopeOrRequestScopeOrThreadScope<T>()
        {
            InIsolatedThreadScopeOrRequestScopeOrThreadScope(this.Kernel.Bind<T>().ToSelf());
        }

        public static IBindingNamedWithOrOnSyntax<T> InIsolatedThreadScopeOrRequestScopeOrThreadScope<T>(IBindingInSyntax<T> syntax)
        {
            var isolatedThreadScopeCallback = GetIsolatedThreadScopeCallback();
            var threadScopeCallback = GetScopeCallback(syntax.InThreadScope());

            return syntax.InScope(context
                => isolatedThreadScopeCallback.Invoke(context)
                   ?? threadScopeCallback.Invoke(context));
        }

        private static Func<IContext, object> GetScopeCallback<T>(IBindingNamedWithOrOnSyntax<T> requestScopeSyntax)
        {
            return requestScopeSyntax.BindingConfiguration.ScopeCallback;
        }

        private static Func<IContext, object> GetIsolatedThreadScopeCallback()
        {
            return context => Thread.CurrentThread.AsIsolatedThread();
        }
    }

    public class NinjectModuleContext : IModuleContext
    {
        private readonly IContext context;

        public NinjectModuleContext(IContext context)
        {
            this.context = context;
        }

        public T Resolve<T>()
        {
            return context.Kernel.Get<T>();
        }

        public Type MemberDeclaringType => context.Request.Target?.Member.DeclaringType;

        public T Inject<T>()
        {
            return context.Kernel.Get<T>();
        }

        public T Get<T>()
        {
            return context.Kernel.Get<T>();
        }

        public T Get<T>(string name)
        {
            return context.Kernel.Get<T>(name);
        }

        public object Get(Type type)
        {
            return context.Kernel.Get(type);
        }

        public object GetServiceWithGenericType(Type type, params Type[] genericType)
        {
            var generic = type.MakeGenericType(genericType);
            return context.Kernel.GetService(generic);
        }

        public Type GetGenericArgument()
        {
            return context.GenericArguments[0];
        }

        public Type[] GetGenericArguments()
        {
            return context.GenericArguments;
        }
    }

    public class NinjectConstructorContext : IConstructorContext
    {
        private readonly IContext context;

        public NinjectConstructorContext(IContext context)
        {
            this.context = context;
        }

        public T Inject<T>()
        {
            return context.Kernel.Get<T>();
        }
    }
}
