using System;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Shared.Web.Filters;


namespace WB.UI.Supervisor.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes(new TypedDirectRouteProvider());

            config.TypedRoute("api/interviewer/v1", c => c.Action<InterviewerApiV1Controller>(x => x.Get()));
            config.TypedRoute("api/interviewer/v1/latestversion", c => c.Action<InterviewerApiV1Controller>(x => x.GetLatestVersion()));
            config.TypedRoute("api/interviewer/v1/tabletInfo", c => c.Action<InterviewerApiV1Controller>(x => x.PostTabletInformation(Param.Any<TabletInformationPackage>())));
            config.TypedRoute("api/interviewer/v1/devices/current/{id}/{version}", c => c.Action<DevicesApiV1Controller>(x => x.CanSynchronize(Param.Any<string>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v1/devices/link/{id}/{version:int}", c => c.Action<DevicesApiV1Controller>(x => x.LinkCurrentInterviewerToDevice(Param.Any<string>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v1/users/current", c => c.Action<UsersApiV1Controller>(x => x.Current()));
            config.TypedRoute("api/interviewer/v1/users/hasdevice", c => c.Action<UsersApiV1Controller>(x => x.HasDevice()));
            config.TypedRoute("api/interviewer/v1/questionnaires/census", c => c.Action<QuestionnairesApiV1Controller>(x => x.Census()));
            config.TypedRoute("api/interviewer/v1/questionnaires/{id:guid}/{version:int}", c => c.Action<QuestionnairesApiV1Controller>(x => x.Get(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v1/questionnaires/{id:guid}/{version:int}/assembly", c => c.Action<QuestionnairesApiV1Controller>(x => x.GetAssembly(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v1/questionnaires/{id:guid}/{version:int}/logstate", c => c.Action<QuestionnairesApiV1Controller>(x => x.LogQuestionnaireAsSuccessfullyHandled(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v1/questionnaires/{id:guid}/{version:int}/assembly/logstate", c => c.Action<QuestionnairesApiV1Controller>(x => x.LogQuestionnaireAssemblyAsSuccessfullyHandled(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v1/packages/{lastPackageId?}", c => c.Action<InterviewsApiV1Controller>(x => x.GetPackages(Param.Any<string>())));
            config.TypedRoute("api/interviewer/v1/package/{id}", c => c.Action<InterviewsApiV1Controller>(x => x.GetPackage(Param.Any<string>())));
            config.TypedRoute("api/interviewer/v1/package/{id}/logstate", c => c.Action<InterviewsApiV1Controller>(x => x.LogPackageAsSuccessfullyHandled(Param.Any<string>())));
            config.TypedRoute("api/interviewer/v1/package/{id:guid}", c => c.Action<InterviewsApiV1Controller>(x => x.PostPackage(Param.Any<Guid>(), Param.Any<string>())));
            config.TypedRoute("api/interviewer/v1/interviews", c => c.Action<InterviewsApiV1Controller>(x => x.Get()));
            config.TypedRoute("api/interviewer/v1/interviews/{id:guid}", c => c.Action<InterviewsApiV1Controller>(x => x.Details(Param.Any<Guid>())));
            config.TypedRoute("api/interviewer/v1/interviews/{id:guid}/logstate", c => c.Action<InterviewsApiV1Controller>(x => x.LogInterviewAsSuccessfullyHandled(Param.Any<Guid>())));
            config.TypedRoute("api/interviewer/v1/interviews/{id:guid}", c => c.Action<InterviewsApiV1Controller>(x => x.Post(Param.Any<Guid>(), Param.Any<string>())));
            config.TypedRoute("api/interviewer/v1/interviews/{id:guid}/image", c => c.Action<InterviewsApiV1Controller>(x => x.PostImage(Param.Any<PostFileRequest>())));


            config.TypedRoute("api/interviewer/v2", c => c.Action<InterviewerApiV2Controller>(x => x.Get()));
            config.TypedRoute("api/interviewer/v2/latestversion", c => c.Action<InterviewerApiV2Controller>(x => x.GetLatestVersion()));
            config.TypedRoute("api/interviewer/v2/tabletInfo", c => c.Action<InterviewerApiV2Controller>(x => x.PostTabletInformation(Param.Any<TabletInformationPackage>())));
            config.TypedRoute("api/interviewer/v2/devices/current/{id}/{version}", c => c.Action<DevicesApiV2Controller>(x => x.CanSynchronize(Param.Any<string>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/devices/link/{id}/{version:int}", c => c.Action<DevicesApiV2Controller>(x => x.LinkCurrentInterviewerToDevice(Param.Any<string>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/users/current", c => c.Action<UsersApiV2Controller>(x => x.Current()));
            config.TypedRoute("api/interviewer/v2/users/hasdevice", c => c.Action<UsersApiV2Controller>(x => x.HasDevice()));
            config.TypedRoute("api/interviewer/v2/questionnaires/census", c => c.Action<QuestionnairesApiV2Controller>(x => x.Census()));
            config.TypedRoute("api/interviewer/v2/questionnaires/{id:guid}/{version:int}/{contentVersion:long}", c => c.Action<QuestionnairesApiV2Controller>(x => x.Get(Param.Any<Guid>(), Param.Any<int>(), Param.Any<long>())));
            config.TypedRoute("api/interviewer/v2/questionnaires/{id:guid}/{version:int}/assembly", c => c.Action<QuestionnairesApiV2Controller>(x => x.GetAssembly(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/questionnaires/{id:guid}/{version:int}/logstate", c => c.Action<QuestionnairesApiV2Controller>(x => x.LogQuestionnaireAsSuccessfullyHandled(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/questionnaires/{id:guid}/{version:int}/assembly/logstate", c => c.Action<QuestionnairesApiV2Controller>(x => x.LogQuestionnaireAssemblyAsSuccessfullyHandled(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/interviews", c => c.Action<InterviewsApiV2Controller>(x => x.Get()));
            config.TypedRoute("api/interviewer/v2/interviews/{id:guid}", c => c.Action<InterviewsApiV2Controller>(x => x.Details(Param.Any<Guid>())));
            config.TypedRoute("api/interviewer/v2/interviews/{id:guid}/logstate", c => c.Action<InterviewsApiV2Controller>(x => x.LogInterviewAsSuccessfullyHandled(Param.Any<Guid>())));
            config.TypedRoute("api/interviewer/v2/interviews/{id:guid}", c => c.Action<InterviewsApiV2Controller>(x => x.Post(Param.Any<InterviewPackageApiView>())));
            config.TypedRoute("api/interviewer/v2/interviews/{id:guid}/image", c => c.Action<InterviewsApiV2Controller>(x => x.PostImage(Param.Any<PostFileRequest>())));
            config.TypedRoute("api/interviewer/v2/questionnaires/{id:guid}/{version:int}/attachments", c => c.Action<QuestionnairesApiV2Controller>(x => x.GetAttachments(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/attachments/{id}", c => c.Action<AttachmentsApiV2Controller>(x => x.GetAttachmentContent(Param.Any<string>())));

            config.Routes.MapHttpRoute(
                name: "DefaultApiWithAction",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
            
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
            config.MessageHandlers.Add(new DecompressionHandler());
        }
    }
}