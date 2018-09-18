using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.API.DataCollection;
using WB.UI.Headquarters.API.DataCollection.Interviewer;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v2;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v3;
using WB.UI.Headquarters.API.DataCollection.Supervisor;
using WB.UI.Headquarters.API.DataCollection.Supervisor.v1;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Shared.Web.Compression;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters
{
    [Localizable(false)]
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            #pragma warning disable 4014

            config.MapHttpAttributeRoutes(new TypedDirectRouteProvider());

            InterviewerCommonWebApiConfig.Register(config);
            InterviewerV2WebApiConfig.Register(config);
            InterviewerV3WebApiConfig.Register(config);
            SupervisorV1WebApiConfig.Register(config);

            config.Filters.Add(new UnhandledExceptionFilter());
            config.MessageHandlers.Add(new CompressionHandler());

            config.Routes.MapHttpRoute("DefaultApiWithAction", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });

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

            config.MessageHandlers.Add(new EnforceHttpsHandler());

            config.Services.Add(typeof(IExceptionLogger), new NLogExceptionLogger());

            config.EnsureInitialized();
            #pragma warning restore 4014
        }
    }
}
