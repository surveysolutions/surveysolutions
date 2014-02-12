using System.Web.Http;
using System.Web.Http.Dispatcher;
using Web.Supervisor.Code.MessageHandler;


namespace Web.Supervisor.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            //config.MessageHandlers.Add(new BasicAuthMessageHandler());
            
            config.Routes.MapHttpRoute(
            name: "BasicAuthApi",
            routeTemplate: "apis/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional },
            constraints: null,
            handler: new BasicAuthMessageHandler() { InnerHandler = new HttpControllerDispatcher(config) });

            config.Routes.MapHttpRoute(
            name: "BasicAuthApiWithAction",
            routeTemplate: "apis/{controller}/{action}/{id}",
            defaults: new { id = RouteParameter.Optional },
            constraints: null,
            handler: new BasicAuthMessageHandler() { InnerHandler = new HttpControllerDispatcher(config) });

            /*config.Routes.MapHttpRoute(
                name: "TokenAuthApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { controller = "controller2" },
                handler: new TokenAuthMessageHandler() { InnerHandler = new HttpControllerDispatcher(config) }
            );*/


            config.Routes.MapHttpRoute(
                name: "DefaultApiWithAction",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}