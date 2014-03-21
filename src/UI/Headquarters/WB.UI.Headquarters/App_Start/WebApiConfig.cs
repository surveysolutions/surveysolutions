using System.Web.Http;
using WB.UI.Headquarters.Api.ActionFilters;

namespace WB.UI.Headquarters.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Filters.Add(new UnhandledExceptionFilter());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
