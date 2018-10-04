using System;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
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

        public void BindInPerUnitOfWorkOrPerRequestScope<TInterface, TImplementation>() where TImplementation : TInterface
        {
            containerBuilder.RegisterType<TImplementation>().As<TInterface>()
                .InstancePerMatchingLifetimeScope(
                    AutofacServiceLocatorAdapterWithChildrenScopes.UnitOfWorkScope,
                    MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
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
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new WebApiActionFilter<T>(c.Resolve<T>()))
                .AsWebApiActionFilterFor<ApiController>()
                .InstancePerRequest();
        }

        public void BindMvcActionFilter<T>() where T: System.Web.Mvc.ActionFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new MvcActionFilter(typeof(T)))
                .AsActionFilterFor<Controller>()
                .InstancePerRequest();
        }

        public void BindWebApiExceptionFilter<T>() where T : System.Web.Http.Filters.ExceptionFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new WebApiExceptionFilter<T>(c.Resolve<T>()))
                .AsWebApiExceptionFilterFor<ApiController>()
                .InstancePerRequest();
        }

        public void BindMvcExceptionFilter<T>() where T : IExceptionFilter
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new MvcExceptionFilter<T>(c.Resolve<T>()))
                .AsExceptionFilterFor<Controller>()
                .InstancePerRequest();
        }

        public void BindWebApiAuthorizationFilter<T>() where T : System.Web.Http.Filters.AuthorizationFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new WebApiAuthorizationFilter<T>(c.Resolve<T>()))
                .AsWebApiAuthorizationFilterFor<ApiController>()
                .InstancePerRequest();
        }

        public void BindMvcAuthorizationFilter<T>() where T : IAuthorizationFilter
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new MvcAuthorizationFilter<T>(c.Resolve<T>()))
                .AsAuthorizationFilterFor<Controller>()
                .InstancePerRequest();
        }

        public void BindMvcActionFilter<T>(FilterScope filterScope, int? order) where T : ActionFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new MvcActionFilter(typeof(T)))
                .AsActionFilterFor<Controller>()
                .InstancePerRequest();
        }

        public void BindMvcFilterWhenActionMethodHasNoAttribute<T, TAttribute>(int order = -1)
            where T : System.Web.Mvc.ActionFilterAttribute
            where TAttribute : Attribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new MvcActionFilterWhenActionMethodHasNoTransactionAttribute(c.Resolve<T>(), typeof(TAttribute)))
                .AsActionFilterFor<Controller>(order)
                .InstancePerRequest();
        }

        public void BindWebApiFilterWhenActionMethodHasNoAttribute<T, TAttribute>() 
            where T : System.Web.Http.Filters.ActionFilterAttribute
            where TAttribute : Attribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new WebApiActionFilterWhenActionMethodHasNoAttribute<T, TAttribute>(c.Resolve<T>()))
                .AsWebApiActionFilterFor<ApiController>()
                .InstancePerRequest();
        }

        public void BindWebApiFilterWhenControllerHasAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, int? order = null) where T : IFilter
        {
            throw new NotImplementedException();
        }

        public void BindWebApiAuthorizationFilterWhenControllerHasAttribute<T, TAttribute>() where T : System.Web.Http.Filters.IAuthorizationFilter where TAttribute : Attribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new WebApiAuthorizationFilterWhenActionMethodHasAttribute<T, TAttribute>(c.Resolve<T>()))
                .AsWebApiAuthorizationFilterFor<ApiController>()
                .InstancePerRequest();
        }

        public void BindWebApiAuthorizationFilterWhenControllerHasAttribute<T, TAttribute>(ConstructorArgument constructorArgument) 
            where T : System.Web.Http.Filters.IAuthorizationFilter
            where TAttribute : Attribute
        {
            throw new NotImplementedException();
//            containerBuilder.RegisterType<T>().AsSelf().WithParameter(constructorArgument.Name, constructorArgument.Value).InstancePerLifetimeScope();
//            containerBuilder.Register(c => new WebApiAuthorizationFilterWhenActionMethodHasAttribute<T, TAttribute>(c.Resolve<T>()))
//                .AsWebApiAuthorizationFilterFor<ApiController>()
//                .InstancePerRequest();
        }
    }
}
