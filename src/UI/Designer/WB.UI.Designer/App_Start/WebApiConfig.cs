using System.Web.Http;

namespace WB.UI.Designer
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //Temporary comment Web API auth during investigation how it works with Angular
            //config.MessageHandlers.Add(new HttpsVerifier());
            //config.MessageHandlers.Add(new BasicAuthMessageHandler());

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