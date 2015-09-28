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
            
            
            //support json for browser requests
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            config.MessageHandlers.Add(new EnforceHttpsHandler());
        }
    }
}