﻿using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Elmah.Contrib.WebApi;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.MessageHandlers;
using WB.UI.Designer.Filters;

namespace WB.UI.Designer
{
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

            config.MapHttpAttributeRoutes();

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