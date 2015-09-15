using System.Net.Http.Headers;
using System.Web.Http;
using WB.UI.Shared.Web.Filters;


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

            config.Routes.MapHttpRoute("InterviewerApplicationApi", "api/interviewer/{apiVersion}/{action}", new { controller = "interviewer" });

            config.Routes.MapHttpRoute("InterviewerUsersApiWithAction", "api/interviewer/{apiVersion}/users/{action}/{id}", new { controller = "InterviewerUsers", id = RouteParameter.Optional });
            config.Routes.MapHttpRoute("InterviewerUsersApi", "api/interviewer/{apiVersion}/users/{id}", new { controller = "InterviewerUsers", id = RouteParameter.Optional });

            config.Routes.MapHttpRoute("InterviewerDevicesApiWithAction", "api/interviewer/{apiVersion}/devices/{action}/{id}", new { controller = "InterviewerDevices", id = RouteParameter.Optional });
            config.Routes.MapHttpRoute("InterviewerDevicesApi", "api/interviewer/{apiVersion}/devices/{id}", new { controller = "InterviewerDevices", id = RouteParameter.Optional });

            config.Routes.MapHttpRoute("InterviewerQuestionnairesApiWithAction", "api/interviewer/{apiVersion}/questionnaires/{action}/{id}", new { controller = "InterviewerQuestionnaires", id = RouteParameter.Optional });
            config.Routes.MapHttpRoute("InterviewerQuestionnairesApi", "api/interviewer/{apiVersion}/questionnaires/{id}", new { controller = "InterviewerQuestionnaires", id = RouteParameter.Optional });

            config.Routes.MapHttpRoute("InterviewerInterviewsApiWithAction", "api/interviewer/{apiVersion}/interviews/{action}/{id}", new { controller = "InterviewerInterviews", id = RouteParameter.Optional });
            config.Routes.MapHttpRoute("InterviewerInterviewsApi", "api/interviewer/{apiVersion}/interviews/{id}", new { controller = "InterviewerInterviews", id = RouteParameter.Optional });

            //support json for browser requests
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            config.MessageHandlers.Add(new EnforceHttpsHandler());
        }
    }
}