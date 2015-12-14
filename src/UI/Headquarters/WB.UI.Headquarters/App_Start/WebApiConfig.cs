using System.Net.Http.Headers;
using System.Web.Http;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Headquarters.API.Formatters;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Filters;

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