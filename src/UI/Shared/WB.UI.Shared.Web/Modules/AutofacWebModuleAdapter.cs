using System;
using System.Web.Http.Filters;
using Autofac;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using FilterScope = System.Web.Mvc.FilterScope;

namespace WB.UI.Shared.Web.Modules
{
    //todo:AF clean merge code from AutofacModuleAdapter
    public class AutofacWebModuleAdapter : Module, IWebIocRegistry
    {
        private readonly IWebModule module;
        private ContainerBuilder containerBuilder;

        public AutofacWebModuleAdapter(IWebModule module)
        {
            this.module = module;
        }

        protected override void Load(ContainerBuilder builder)
        {
            containerBuilder = builder;
            this.module.Load(this);
        }

        void IIocRegistry.Bind<TInterface, TImplementation>()
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>();
        }

        public void Bind(Type @interface, Type implementation)
        {
            if (@interface.IsGenericType && implementation.IsGenericType)
            {
                containerBuilder.RegisterGeneric(implementation).As(@interface);
            }
            else
            {
                containerBuilder.RegisterType(implementation).As(@interface);
            }
        }

        public void Bind<TInterface1, TInterface2, TImplementation>() where TImplementation : TInterface1, TInterface2
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface1, TInterface2>();
        }

        public void Bind<TInterface, TImplementation>(params ConstructorArgument[] constructorArguments) where TImplementation : TInterface
        {
            var registrationBuilder = containerBuilder.RegisterType<TImplementation>().As<TInterface>();
            foreach (var constructorArgument in constructorArguments)
            {
                registrationBuilder.WithParameter(constructorArgument.Name, constructorArgument.Value);
            }
        }

        public void Bind<TImplementation>()
        {
            containerBuilder.RegisterType<TImplementation>();
        }

        public void BindWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>()
                .WithParameter(argumentName, argumentValue);
        }

        public void BindGeneric(Type implemenation)
        {
            containerBuilder.RegisterGeneric(implemenation);
        }

        public void Unbind<T>()
        {
        }

        public bool HasBinding<T>()
        {
            return false;
        }

        public void BindInPerLifetimeScope<T1, T2>() where T2 : T1
        {
            containerBuilder.RegisterType<T2>().As<T1>().InstancePerLifetimeScope();
        }


        void IIocRegistry.BindAsSingleton<TInterface, TImplementation>()
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>().SingleInstance();
        }

        public void BindAsSingleton<TInterface1, TInterface2, TImplementation>() where TImplementation : TInterface2, TInterface1
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface1, TInterface1>().SingleInstance();
        }

        void IIocRegistry.BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue)
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>()
                .WithParameter(argumentName, argumentValue).SingleInstance();
        }

        void IIocRegistry.BindToRegisteredInterface<TInterface, TRegisteredInterface>()
        {
            containerBuilder.Register<TInterface>(c => c.Resolve<TRegisteredInterface>());
        }

        public void BindToMethod<T>(Func<T> func, string name = null)
        {
            if (name == null)
            {
                BindToMethod(func);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void BindToMethod<T>(Func<IModuleContext, T> func, string name = null)
        {
            containerBuilder.Register((ctx, p) => func(new AutofacModuleContext(ctx, p)));
        }

        public void BindToMethodInSingletonScope<T>(Func<IModuleContext, T> func, string named = null)
        {
            containerBuilder.Register((ctx, p) => func(new AutofacModuleContext(ctx, p))).SingleInstance();
        }

        public void BindToMethodInSingletonScope(Type @interface, Func<IModuleContext, object> func)
        {
            containerBuilder.Register((ctx, p) => func(new AutofacModuleContext(ctx, p))).SingleInstance();
        }

        public void BindToMethodInRequestScope<T>(Func<IModuleContext, T> func)
        {
            containerBuilder.Register((ctx, p) => func(new AutofacModuleContext(ctx, p))).InstancePerRequest();
        }

        public void BindToMethod<T>(Func<T> func)
        {
            containerBuilder.Register(ctx => func());
        }

        public void BindToConstant<T>(Func<T> func)
        {
            containerBuilder.Register(ctx => func()).SingleInstance();
        }

        public void BindToConstant<T>(Func<IModuleContext, T> func)
        {
            containerBuilder.Register((ctx, p) => func(new AutofacModuleContext(ctx, p))).SingleInstance();
        }
       
        public void BindAsSingleton(Type @interface, Type implementation)
        {
            if (@interface.IsGenericType)
            {
                containerBuilder.RegisterGeneric(implementation).As(@interface).SingleInstance();
            }
            else
            {
                containerBuilder.RegisterType(implementation).As(@interface).SingleInstance();
            }
        }
       
        /// ////////////////////////////////////////////////////////////
       


        public void BindMvcFilter<T>(FilterScope filterScope, int? order)
        {
            containerBuilder.RegisterType<T>().PropertiesAutowired().InstancePerLifetimeScope();
            //this.Kernel.BindFilter<T>(filterScope, order);
        }

        public void BindMvcFilterWhenActionMethodHasNoAttribute<T, TAttribute>(FilterScope filterScope, int? order)
        {

            containerBuilder.RegisterType<T>().PropertiesAutowired().InstancePerLifetimeScope();
            //throw new NotImplementedException();
        }

        public void BindHttpFilter<T>(System.Web.Http.Filters.FilterScope filterScope, int? order) where T : IFilter
        {
            containerBuilder.RegisterType<T>().PropertiesAutowired().InstancePerLifetimeScope();

            //throw new NotImplementedException();
        }

        public void BindHttpFilterWhenActionMethodHasNoAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, int? order = null) where T : IFilter
        {
            //containerBuilder.Register(c => new T()).AsActionFilterFor<Controller>().InstancePerHttpRequest();

            containerBuilder.RegisterType<T>().PropertiesAutowired().InstancePerLifetimeScope();
            //throw new NotImplementedException();
        }

        public void BindHttpFilterWhenControllerHasAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, int? order = null) where T : IFilter
        {
            containerBuilder.RegisterType<T>().PropertiesAutowired().InstancePerLifetimeScope();
            //throw new NotImplementedException();
        }

        public void BindHttpFilterWhenControllerHasAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope,
            ConstructorArgument constructorArgument) where T : IFilter
        {
            containerBuilder.RegisterType<T>().PropertiesAutowired().InstancePerLifetimeScope();

            //throw new NotImplementedException();
            //translate from
            //this.containerBuilder.BindHttpFilter<T>(filterScope).WhenControllerHas<TAttribute>();
        }
    }
}
