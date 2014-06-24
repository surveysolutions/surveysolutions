using System.Net.Http.Headers;
using System.Web.Http;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Headquarters.API.Formatters;

namespace WB.UI.Headquarters
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Filters.Add(new UnhandledExceptionFilter());
            config.Formatters.Add(new SyndicationFeedFormatter());

            config.Routes.MapHttpRoute("DefaultApiWithAction", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            //support json for browser requests
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }
    }
}