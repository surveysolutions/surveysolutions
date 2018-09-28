using System;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.UI.Shared.Web.Modules.Filters;
using ActionFilterAttribute = System.Web.Mvc.ActionFilterAttribute;
using FilterScope = System.Web.Mvc.FilterScope;
using IAuthorizationFilter = System.Web.Mvc.IAuthorizationFilter;
using IExceptionFilter = System.Web.Mvc.IExceptionFilter;


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

        public void BindInPerUnitOfWorkScope<TInterface, TImplementation>() where TImplementation : TInterface
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>().InstancePerMatchingLifetimeScope(AutofacServiceLocatorAdapterWithChildrenScopes.UnitOfWorkScope);
        }

        public void BindWithConstructorArgumentInPerLifetimeScope<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>()
                .WithParameter(argumentName, argumentValue).InstancePerLifetimeScope();
        }

        public void BindGeneric(Type implemenation)
        {
            containerBuilder.RegisterGeneric(implemenation);
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
       


        public void BindWebApiFilter<T>() where T: System.Web.Http.Filters.ActionFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerLifetimeScope();
            containerBuilder.Register(c => new AutofacActionFilter<T>(c.Resolve<T>()))
                .AsWebApiActionFilterFor<ApiController>()
                .InstancePerLifetimeScope();
        }

        public void BindMvcFilter<T>() where T: System.Web.Mvc.ActionFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerLifetimeScope();
            containerBuilder.Register(c => new AutofacActionFilter<T>(c.Resolve<T>()))
                .AsActionFilterFor<Controller>()
                .InstancePerLifetimeScope();
        }

        public void BindWebApiExceptionFilter<T>() where T : System.Web.Http.Filters.ExceptionFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerLifetimeScope();
            containerBuilder.Register(c => new AutofacExceptionFilter<T>(c.Resolve<T>()))
                .AsWebApiExceptionFilterFor<ApiController>()
                .InstancePerLifetimeScope();
        }

        public void BindMvcExceptionFilter<T>() where T : IExceptionFilter
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerLifetimeScope();
            containerBuilder.Register(c => new AutofacExceptionFilter<T>(c.Resolve<T>()))
                .AsExceptionFilterFor<Controller>()
                .InstancePerLifetimeScope();
        }

        public void BindWebApiAuthorizationFilter<T>() where T : System.Web.Http.Filters.AuthorizationFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerLifetimeScope();
            containerBuilder.Register(c => new WebApiAuthorizationFilter<T>(c.Resolve<T>()))
                .AsWebApiAuthorizationFilterFor<ApiController>()
                .InstancePerLifetimeScope();
        }

        public void BindMvcAuthorizationFilter<T>() where T : IAuthorizationFilter
        {
            throw new NotImplementedException();
        }

        public void BindMvcFilter<T>(FilterScope filterScope, int? order) where T : ActionFilterAttribute
        {
            throw new NotImplementedException();
        }

        void IWebIocRegistry.BindMvcFilterWhenActionMethodHasNoAttribute<T, TAttribute>(FilterScope filterScope, int? order)
        {
            throw new NotImplementedException();
        }

        public void BindMvcFilterWhenActionMethodHasNoAttribute<T, TAttribute>(System.Web.Mvc.FilterScope filterScope, int? order)
        {
            throw new NotImplementedException();
        }

        public void BindMvcFilter<T>(System.Web.Http.Filters.FilterScope filterScope, int? order) where T : IFilter
        {
            throw new NotImplementedException();
        }

        public void BindWebApiFilterWhenActionMethodHasNoAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, int? order = null) where T : IFilter
        {
            throw new NotImplementedException();
        }

        public void BindWebApiFilterWhenControllerHasAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, int? order = null) where T : IFilter
        {
            throw new NotImplementedException();
        }

        public void BindWebApiFilterWhenControllerHasAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope,
            ConstructorArgument constructorArgument) where T : IFilter
        {
            throw new NotImplementedException();
        }
    }
}
