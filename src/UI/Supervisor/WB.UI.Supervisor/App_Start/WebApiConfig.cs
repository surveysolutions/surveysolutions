﻿using System.Web.Http;
using System.Web.Http.Dispatcher;
using WB.UI.Supervisor.Code.MessageHandler;


namespace WB.UI.Supervisor.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            
            config.Routes.MapHttpRoute(
                name: "DefaultApiWithAction",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            //config.MessageHandlers.Add(new BasicAuthMessageHandler());
        }
    }
}