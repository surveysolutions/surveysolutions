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

            //Real Api
            config.Routes.MapHttpRoute("InterviewsApiAction", "api/{apiVersion}/interviews/{action}", new { controller = "interviews" });
            config.Routes.MapHttpRoute("DefaultApiWithActionA", "api/{apiVersion}/interviews/{action}/{id}", new { controller = "interviews", id = RouteParameter.Optional });

            config.Routes.MapHttpRoute("UsersApiAction", "api/{apiVersion}/users/{action}", new { controller = "users" });
            config.Routes.MapHttpRoute("UsersApiWithActionA", "api/{apiVersion}/users/{action}/{id}", new { controller = "users", id = RouteParameter.Optional });

            config.Routes.MapHttpRoute("SupervisorsApiAction", "api/{apiVersion}/supervisors/{action}", new { controller = "users" });
            config.Routes.MapHttpRoute("SupervisorsApiWithActionA", "api/{apiVersion}/supervisors/{action}/{id}", new { controller = "users", id = RouteParameter.Optional });

            config.Routes.MapHttpRoute("InterviewersApiAction", "api/{apiVersion}/interviewers/{action}", new { controller = "users" });
            config.Routes.MapHttpRoute("InterviewersApiWithActionA", "api/{apiVersion}/interviewers/{action}/{id}", new { controller = "users", id = RouteParameter.Optional });

            config.Routes.MapHttpRoute("QuestionnairesApiAction", "api/{apiVersion}/questionnaires/{action}", new { controller = "questionnaires" });
            config.Routes.MapHttpRoute("QuestionnairesApiWithActionA", "api/{apiVersion}/questionnaires/{action}/{id}", new { controller = "questionnaires", id = RouteParameter.Optional });
            //support json for browser requests
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            config.MessageHandlers.Add(new EnforceHttpsHandler());
        }
    }
}