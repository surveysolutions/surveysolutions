using System.Linq;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Routing;
using StackExchange.Exceptional;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.MessageHandlers;
using WB.UI.Shared.Web.Compression;

namespace WB.UI.Designer
{

    public class CentralizedPrefixProvider : DefaultDirectRouteProvider
    {
        private readonly string _centralizedPrefix;

        private static readonly string[] versionedNamespaces =
        {
            typeof(Api.Tester.QuestionnairesController).Namespace
        };

        public CentralizedPrefixProvider(string centralizedPrefix)
        {
            _centralizedPrefix = centralizedPrefix;
        }

        protected override string GetRoutePrefix(HttpControllerDescriptor controllerDescriptor)
        {
            var existingPrefix = base.GetRoutePrefix(controllerDescriptor);

            if (versionedNamespaces.Contains(controllerDescriptor.ControllerType.Namespace))
            {
                if (existingPrefix == null) return _centralizedPrefix;

                return $"{this._centralizedPrefix}/{existingPrefix}";
            }

            return existingPrefix;
        }
    }

    public class ExceptionalLogger : IExceptionLogger
    {
        public async Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            await context.Exception
                .LogAsync(HttpContext.Current);
        }
    }

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Services.Add(typeof(IExceptionLogger),  new ExceptionalLogger());

            //Temporary comment Web API auth during investigation how it works with Angular
            //config.MessageHandlers.Add(new BasicAuthMessageHandler());

            if (AppSettings.Instance.IsApiSslVerificationEnabled)
                config.MessageHandlers.Add(new HttpsVerifier());

            //config.Filters.Add(new CustomWebApiAuthorizeFilter());

            config.MessageHandlers.Add(new CompressionHandler());

            config.MapHttpAttributeRoutes(new CentralizedPrefixProvider("api/v{version:int}"));

            config.Routes.MapHttpRoute(
                name: "HQApiWithAction",
                routeTemplate: "api/hq/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                name: "HQApi",
                routeTemplate: "api/hq/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            
            config.Routes.MapHttpRoute(
                name: "VersionedHQApiWithAction",
                routeTemplate: "api/hq/v{version:int}/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                name: "VersionedHQApi",
                routeTemplate: "api/hq/v{version:int}/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "VersionedApiWithAction",
                routeTemplate: "api/v{version:int}/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                name: "VersionedApi",
                routeTemplate: "api/v{version:int}/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApiWithAction",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var xml = config.Formatters.SingleOrDefault(f => f is XmlMediaTypeFormatter);
            if (xml != null) config.Formatters.Remove(xml);
            config.Formatters.Insert(0, new JsonFormatter());
        } 
    }
}
