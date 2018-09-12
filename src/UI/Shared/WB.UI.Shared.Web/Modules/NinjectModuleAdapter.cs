using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http.Filters;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Ninject.Syntax;
using Ninject.Web.Common;
using Ninject.Web.Mvc.FilterBindingSyntax;
using Ninject.Web.WebApi.FilterBindingSyntax;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Ioc;
using WB.Infrastructure.Native.Threading;
using FilterScope = System.Web.Mvc.FilterScope;

namespace WB.UI.Shared.Web.Modules
{
    public  class NinjectModuleAdapter<TModule> : NinjectModuleAdapter
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

    public class NinjectWebModuleAdapter<TModule> : NinjectModuleAdapter
        where TModule : IWebModule
    {
        private readonly TModule module;

        public NinjectWebModuleAdapter(TModule module)
        {
            this.module = module;
        }

        public override void Load()
        {
            this.module.Load(this);
        }

        public override string Name => this.module.ToString();
    }

    public abstract class NinjectModuleAdapter : NinjectModule, IWebIocRegistry
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

        public void BindWithConstructorArgumentInPerLifetimeScope<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>()
                .InRequestScope()
                .WithConstructorArgument(argumentName, argumentValue);
        }

        void IIocRegistry.BindAsSingleton<TInterface, TImplementation>()
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope();
        }

        public void BindAsSingleton<TInterface1, TInterface2, TImplementation>() where TImplementation : TInterface2, TInterface1
        {
            this.Kernel.Bind<TInterface1, TInterface2>().To<TImplementation>().InSingletonScope();
        }

        void IIocRegistry.BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue)
        {
            this.Kernel.Bind<TInterface>().To<TImplementation>().InSingletonScope()
                .WithConstructorArgument(argumentName, argumentValue);
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
            this.Kernel.Bind<T>().ToMethod(ctx => func(new NinjectModuleContext(ctx))).InRequestScope();
        }

        void IIocRegistry.BindToConstant<T>(Func<T> func)
        {
            this.Kernel.Bind<T>().ToMethod(ctx => func()).InSingletonScope();
        }

        void IIocRegistry.BindToConstant<T>(Func<IModuleContext, T> func)
        {
            this.Kernel.Bind<T>().ToMethod(ctx => func(new NinjectModuleContext(ctx))).InSingletonScope();
        }

        /*public void BindToConstructorInSingletonScope<T>(Func<IConstructorContext, T> func)
        //public void BindToConstructorInSingletonScope<T>(Func<IConstructorContext, T> func)
        {
            //this.Kernel.Bind<T>().ToConstructor(ctx => func(new NinjectConstructorContext(ctx))).InSingletonScope();
            this.Kernel.Bind<T>().ToMethod(ctx => func(new NinjectConstructorContext(ctx))).InSingletonScope();
        }*/

        void IIocRegistry.BindAsSingleton(Type @interface, Type implementation)
        {
            this.Kernel.Bind(@interface).To(implementation).InSingletonScope();
        }

        void IIocRegistry.BindGeneric(Type implementation)
        {
            this.Kernel.Bind(implementation);
        }

        void IWebIocRegistry.BindMvcFilter<T>(System.Web.Mvc.FilterScope filterScope, int? order)
        {
            this.Kernel.BindFilter<T>(filterScope, order);
        }
        
        void IWebIocRegistry.BindMvcFilterWhenActionMethodHasNoAttribute<T, TAttribute>(System.Web.Mvc.FilterScope filterScope, int? order)
        {
            this.Kernel.BindFilter<T>(filterScope, order).WhenActionMethodHasNo<TAttribute>();
        }

        void IWebIocRegistry.BindHttpFilter<T>(System.Web.Http.Filters.FilterScope filterScope, int? order) 
        {
            this.Kernel.BindHttpFilter<T>(filterScope);
        }

        void IWebIocRegistry.BindHttpFilterWhenActionMethodHasNoAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, int? order) 
        {
            this.Kernel.BindHttpFilter<T>(filterScope)
                .When((controllerContext, actionDescriptor) => !actionDescriptor.GetCustomAttributes(typeof(TAttribute)).Any());
        }

        public void BindHttpFilterWhenControllerHasAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, int? order = null) where T : IFilter
        {
            this.Kernel.BindHttpFilter<T>(filterScope).WhenControllerHas<TAttribute>();
        }

        public void BindHttpFilterWhenControllerHasAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope,
            ConstructorArgument constructorArgument) where T : IFilter
        {
            this.Kernel.BindHttpFilter<T>(filterScope).WhenControllerHas<TAttribute>()
                .WithConstructorArgument(constructorArgument.Name, ctx => constructorArgument.Value(new NinjectModuleContext(ctx)));
        }

        void IIocRegistry.Unbind<TInterface>()
        {
            this.Kernel.Unbind<TInterface>();
        }

        public bool HasBinding<T>()
        {
            return this.Kernel.GetBindings(typeof(T)).Any();
        }

        public void BindInPerLifetimeScope<T1, T2>() where T2 : T1
        {
            this.Kernel.Bind<T1>().To<T2>().When(x => HttpContext.Current != null).InRequestScope(); //scope: web & webAPI

            this.Kernel.Bind<T1>().To<T2>().When(x => HttpContext.Current == null).InAmbientScope(); //scope: jobs 

            this.Kernel.Bind<T1>().To<T2>().When(x => HttpContext.Current == null).InThreadScope(); // hub

            /*
            this.Kernel.Bind<T1>().To<T2>()
                .When(x => x.Parameters.OfType<NonRequestScopedParameter>().Any())
                .InAmbientScope(); 
            this.Kernel.Bind<T1>().To<T2>()
                .When(x => !x.Parameters.OfType<NonRequestScopedParameter>().Any())
                .InRequestScope();*/
        }

        public static IBindingNamedWithOrOnSyntax<T> InIsolatedThreadScopeOrRequestScopeOrThreadScope<T>(IBindingInSyntax<T> syntax)
        {
            var isolatedThreadScopeCallback = GetIsolatedThreadScopeCallback();
            var requestScopeCallback = GetScopeCallback(syntax.InRequestScope());
            var threadScopeCallback = GetScopeCallback(syntax.InThreadScope());

            return syntax.InScope(context
                => isolatedThreadScopeCallback.Invoke(context)
                   ?? requestScopeCallback.Invoke(context)
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
}
