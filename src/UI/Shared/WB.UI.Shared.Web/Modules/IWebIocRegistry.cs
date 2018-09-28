using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public interface IWebIocRegistry : IIocRegistry
    {
        void BindWebApiFilter<T>() where T : System.Web.Http.Filters.ActionFilterAttribute;

        void BindMvcFilter<T>() where T : System.Web.Mvc.ActionFilterAttribute;

        void BindWebApiExceptionFilter<T>() where T : System.Web.Http.Filters.ExceptionFilterAttribute;
        void BindMvcExceptionFilter<T>() where T : System.Web.Mvc.IExceptionFilter;

        void BindWebApiAuthorizationFilter<T>() where T : System.Web.Http.Filters.AuthorizationFilterAttribute;
        void BindMvcAuthorizationFilter<T>() where T : System.Web.Mvc.IAuthorizationFilter;

        void BindMvcFilter<T>(System.Web.Mvc.FilterScope filterScope, int? order) where T : System.Web.Mvc.ActionFilterAttribute;

        void BindMvcFilterWhenActionMethodHasNoAttribute<T, TAttribute>(System.Web.Mvc.FilterScope filterScope, int? order) where T : System.Web.Mvc.ActionFilterAttribute;

        void BindMvcFilter<T>(System.Web.Http.Filters.FilterScope filterScope, int? order) where T : System.Web.Http.Filters.IFilter;

        void BindWebApiFilterWhenActionMethodHasNoAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, int? order = null) where T : System.Web.Http.Filters.IFilter;

        void BindWebApiFilterWhenControllerHasAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, int? order = null) where T : System.Web.Http.Filters.IFilter;

        void BindWebApiFilterWhenControllerHasAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, ConstructorArgument constructorArgument) where T : System.Web.Http.Filters.IFilter;
    }
}
