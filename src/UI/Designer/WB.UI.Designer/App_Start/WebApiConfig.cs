﻿using System.Linq;
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
            config.Filters.Add(new CustomWebApiAuthorizeFilter());


            config.MessageHandlers.Add(new DecompressionHandler());

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

            config.Formatters.Insert(0, new JsonFormatter());
        } 
    }
}