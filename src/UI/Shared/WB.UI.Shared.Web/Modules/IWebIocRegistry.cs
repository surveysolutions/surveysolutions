using System;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public interface IWebIocRegistry : IIocRegistry
    {
        void BindWebApiFilter<T>() 
            where T : System.Web.Http.Filters.ActionFilterAttribute;

        void BindMvcActionFilter<T>(int order = -1) 
            where T : System.Web.Mvc.ActionFilterAttribute;

        void BindWebApiExceptionFilter<T>()
            where T : System.Web.Http.Filters.ExceptionFilterAttribute;

        void BindMvcExceptionFilter<T>(int order = -1) 
            where T : System.Web.Mvc.IExceptionFilter;

        void BindWebApiAuthorizationFilter<T>() 
            where T : System.Web.Http.Filters.AuthorizationFilterAttribute;

        void BindMvcAuthorizationFilter<T>(int order = -1) 
            where T : System.Web.Mvc.IAuthorizationFilter;

        void BindMvcActionFilterWhenControllerOrActionHasNoAttribute<T, TAttribute>(int order = -1) 
            where T : System.Web.Mvc.ActionFilterAttribute
            where TAttribute : Attribute;

        void BindWebApiActionFilterWhenControllerOrActionHasNoAttribute<T, TAttribute>() 
            where T : System.Web.Http.Filters.ActionFilterAttribute
            where TAttribute : Attribute;

        void BindWebApiAuthorizationFilterWhenControllerOrActionHasAttribute<T, TAttribute>()
            where T : System.Web.Http.Filters.IAuthorizationFilter
            where TAttribute : Attribute;

        void BindWebApiAuthorizationFilterWhenControllerOrActionHasAttribute<T, TAttribute>(ConstructorArgument constructorArgument)
            where T : System.Web.Http.Filters.IAuthorizationFilter
            where TAttribute : Attribute;
    }
}
