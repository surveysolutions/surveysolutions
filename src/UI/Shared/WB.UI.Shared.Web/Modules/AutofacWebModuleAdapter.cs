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
using IAuthorizationFilter = System.Web.Mvc.IAuthorizationFilter;
using IExceptionFilter = System.Web.Mvc.IExceptionFilter;


namespace WB.UI.Shared.Web.Modules
{
    public class AutofacWebModuleAdapter : AutofacModuleAdapter<IWebIocRegistry>, IWebIocRegistry
    {
        public AutofacWebModuleAdapter(IWebModule module) : base(module)
        {
        }


        /// ////////////////////////////////////////////////////////////


        public void BindWebApiFilter<T>() where T: System.Web.Http.Filters.ActionFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new WebApiActionFilter<T>(c.Resolve<T>()))
                .AsWebApiActionFilterFor<ApiController>()
                .InstancePerRequest();
        }

        public void BindMvcActionFilter<T>(int order = -1) where T: System.Web.Mvc.ActionFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new MvcActionFilter(c.Resolve<T>()))
                .AsActionFilterFor<Controller>(order)
                .InstancePerRequest();
        }

        public void BindWebApiExceptionFilter<T>() where T : System.Web.Http.Filters.ExceptionFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new WebApiExceptionFilter<T>(c.Resolve<T>()))
                .AsWebApiExceptionFilterFor<ApiController>()
                .InstancePerRequest();
        }

        public void BindMvcExceptionFilter<T>(int order = -1) where T : IExceptionFilter
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new MvcExceptionFilter<T>(c.Resolve<T>()))
                .AsExceptionFilterFor<Controller>(order)
                .InstancePerRequest();
        }

        public void BindWebApiAuthorizationFilter<T>() where T : System.Web.Http.Filters.AuthorizationFilterAttribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new WebApiAuthorizationFilter<T>(c.Resolve<T>()))
                .AsWebApiAuthorizationFilterFor<ApiController>()
                .InstancePerRequest();
        }

        public void BindMvcAuthorizationFilter<T>(int order = -1) where T : IAuthorizationFilter
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new MvcAuthorizationFilter<T>(c.Resolve<T>()))
                .AsAuthorizationFilterFor<Controller>(order)
                .InstancePerRequest();
        }

        public void BindMvcActionFilterWhenControllerOrActionHasNoAttribute<T, TAttribute>(int order = -1)
            where T : System.Web.Mvc.ActionFilterAttribute
            where TAttribute : Attribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new MvcActionFilterWhenControllerOrActionHasNoAttribute(c.Resolve<T>(), typeof(TAttribute)))
                .AsActionFilterFor<Controller>(order)
                .InstancePerRequest();
        }

        public void BindWebApiActionFilterWhenControllerOrActionHasNoAttribute<T, TAttribute>() 
            where T : System.Web.Http.Filters.ActionFilterAttribute
            where TAttribute : Attribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new WebApiActionFilterWhenControllerOrActionHasNoAttribute<T, TAttribute>(c.Resolve<T>()))
                .AsWebApiActionFilterFor<ApiController>()
                .InstancePerRequest();
        }

        public void BindWebApiAuthorizationFilterWhenControllerOrActionHasAttribute<T, TAttribute>() where T : System.Web.Http.Filters.IAuthorizationFilter where TAttribute : Attribute
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerRequest();
            containerBuilder.Register(c => new WebApiAuthorizationFilterWhenControllerOrActionHasAttribute<T, TAttribute>(c.Resolve<T>()))
                .AsWebApiAuthorizationFilterFor<ApiController>()
                .InstancePerRequest();
        }

        public void BindWebApiAuthorizationFilterWhenControllerOrActionHasAttribute<T, TAttribute>(ConstructorArgument constructorArgument) 
            where T : System.Web.Http.Filters.IAuthorizationFilter
            where TAttribute : Attribute
        {
            containerBuilder.RegisterType<T>().AsSelf().WithParameter(constructorArgument.Name, constructorArgument.Value).InstancePerRequest();
            containerBuilder.Register(c => new WebApiAuthorizationFilterWhenControllerOrActionHasAttribute<T, TAttribute>(c.Resolve<T>()))
                .AsWebApiAuthorizationFilterFor<ApiController>()
                .InstancePerRequest();
        }
    }
}
