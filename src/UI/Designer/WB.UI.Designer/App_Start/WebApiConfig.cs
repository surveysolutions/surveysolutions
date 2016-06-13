using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Routing;
using Elmah.Contrib.WebApi;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.MessageHandlers;
using WB.UI.Designer.Filters;

namespace WB.UI.Designer
{
    public class CentralizedPrefixProvider : DefaultDirectRouteProvider
    {
        private readonly string _centralizedPrefix;

        public CentralizedPrefixProvider(string centralizedPrefix)
        {
            _centralizedPrefix = centralizedPrefix;
        }

        protected override string GetRoutePrefix(HttpControllerDescriptor controllerDescriptor)
        {
            var existingPrefix = base.GetRoutePrefix(controllerDescriptor);
            if (existingPrefix == null) return _centralizedPrefix;

            return $"{this._centralizedPrefix}/{existingPrefix}";
        }
    }

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Services.Add(typeof(IExceptionLogger), new ElmahExceptionLogger());

            //Temporary comment Web API auth during investigation how it works with Angular
            //config.MessageHandlers.Add(new BasicAuthMessageHandler());

            if (AppSettings.Instance.IsApiSslVerificationEnabled)
                config.MessageHandlers.Add(new HttpsVerifier());

            config.Filters.Add(new ApiMaintenanceFilter());

            config.MessageHandlers.Add(new DecompressionHandler());

            config.MapHttpAttributeRoutes(new CentralizedPrefixProvider("api/v{version:int}"));

            config.Routes.MapHttpRoute(
                name: "DefaultApiWithAction",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Formatters.Insert(0, new JsonFormatter());
        } 
    }
}