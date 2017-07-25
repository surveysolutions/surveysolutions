using System;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using WB.Core.GenericSubdomains.Portable.Implementation.Compression;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Headquarters.API.Interviewer;
using WB.UI.Headquarters.API.Interviewer.v2;
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

            RegisterV2Api(config);

            config.TypedRoute(@"api/interviewer", c => c.Action<InterviewerApiController>(x => x.Get()));
            config.TypedRoute(@"api/interviewer/patch/{deviceVersion}", c => c.Action<InterviewerApiController>(x => x.Patch(Param.Any<int>())));
            config.TypedRoute(@"api/interviewer/latestversion", c => c.Action<InterviewerApiController>(x => x.GetLatestVersion()));
            
            config.TypedRoute(@"api/interviewer/extended", c => c.Action<InterviewerApiController>(x => x.GetExtended()));
            config.TypedRoute(@"api/interviewer/extended/patch/{deviceVersion}", c => c.Action<InterviewerApiController>(x => x.PatchExtended(Param.Any<int>())));
            config.TypedRoute(@"api/interviewer/extended/latestversion", c => c.Action<InterviewerApiController>(x => x.GetLatestExtendedVersion()));

            config.TypedRoute(@"api/interviewer/tabletInfo", c => c.Action<InterviewerApiController>(x => x.PostTabletInformation()));
            config.TypedRoute(@"api/interviewer/compatibility/{deviceid}/{deviceSyncProtocolVersion}",
                c => c.Action<InterviewerApiController>(x => x.CheckCompatibility(Param.Any<string>(), Param.Any<int>())));

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

            //support json for browser requests
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            config.MessageHandlers.Add(new EnforceHttpsHandler());

            config.Services.Add(typeof(IExceptionLogger), new NLogExceptionLogger());

            config.EnsureInitialized();
            #pragma warning restore 4014
        }
#pragma warning disable 4014
        private static void RegisterV2Api(HttpConfiguration config)
        {
            config.TypedRoute(@"api/interviewer/v2/devices/info", c => c.Action<DevicesApiV2Controller>(x => x.Info(Param.Any<DeviceInfoApiView>())));
            config.TypedRoute(@"api/interviewer/v2/devices/statistics", c => c.Action<DevicesApiV2Controller>(x => x.Statistics(Param.Any<SyncStatisticsApiView>())));
            config.TypedRoute(@"api/interviewer/v2/devices/exception", c => c.Action<DevicesApiV2Controller>(x => x.UnexpectedException(Param.Any<UnexpectedExceptionApiView>())));

            config.TypedRoute("api/interviewer/v2", c => c.Action<InterviewerApiV2Controller>(x => x.Get()));
            config.TypedRoute("api/interviewer/v2/latestversion", c => c.Action<InterviewerApiV2Controller>(x => x.GetLatestVersion()));


            config.TypedRoute("api/interviewer/v2/tabletInfo",
                c => c.Action<InterviewerApiV2Controller>(x => x.PostTabletInformation(Param.Any<TabletInformationPackage>())));
            config.TypedRoute("api/interviewer/v2/tabletInfoAsFile",
                c => c.Action<InterviewerApiV2Controller>(x => x.PostTabletInformationAsFile()));
            config.TypedRoute("api/interviewer/v2/devices/current/{id}/{version}",
                c => c.Action<DevicesApiV2Controller>(x => x.CanSynchronize(Param.Any<string>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/devices/link/{id}/{version:int}",
                c => c.Action<DevicesApiV2Controller>(x => x.LinkCurrentInterviewerToDevice(Param.Any<string>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/users/login", c => c.Action<UsersApiV2Controller>(x => x.Login(Param.Any<LogonInfo>())));
            config.TypedRoute("api/interviewer/v2/users/current", c => c.Action<UsersApiV2Controller>(x => x.Current()));
            config.TypedRoute("api/interviewer/v2/users/hasdevice", c => c.Action<UsersApiV2Controller>(x => x.HasDevice()));
            config.TypedRoute("api/interviewer/v2/translations/{id}",
                c => c.Action<TranslationsApiV2Controller>(x => x.Get(Param.Any<string>())));
            config.TypedRoute("api/interviewer/v2/companyLogo", c => c.Action<CompanyLogoApiV2Controller>(x => x.Get()));
            config.TypedRoute("api/interviewer/v2/questionnaires/census",
                c => c.Action<QuestionnairesApiV2Controller>(x => x.Census()));
            config.TypedRoute("api/interviewer/v2/questionnaires/list",
                c => c.Action<QuestionnairesApiV2Controller>(x => x.List()));
            config.TypedRoute("api/interviewer/v2/questionnaires/{id:guid}/{version:int}/{contentVersion:long}",
                c => c.Action<QuestionnairesApiV2Controller>(x => x.Get(Param.Any<Guid>(), Param.Any<int>(), Param.Any<long>())));
            config.TypedRoute("api/interviewer/v2/questionnaires/{id:guid}/{version:int}/assembly",
                c => c.Action<QuestionnairesApiV2Controller>(x => x.GetAssembly(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/questionnaires/{id:guid}/{version:int}/logstate",
                c =>
                    c.Action<QuestionnairesApiV2Controller>(
                        x => x.LogQuestionnaireAsSuccessfullyHandled(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/questionnaires/{id:guid}/{version:int}/assembly/logstate",
                c =>
                    c.Action<QuestionnairesApiV2Controller>(
                        x => x.LogQuestionnaireAssemblyAsSuccessfullyHandled(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/questionnaires/{id:guid}/{version:int}/attachments",
                c => c.Action<QuestionnairesApiV2Controller>(x => x.GetAttachments(Param.Any<Guid>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/interviews", c => c.Action<InterviewsApiV2Controller>(x => x.Get()));
            config.TypedRoute("api/interviewer/v2/interviews/{id:guid}",
                c => c.Action<InterviewsApiV2Controller>(x => x.Details(Param.Any<Guid>())));
            config.TypedRoute("api/interviewer/v2/interviews/{id:guid}/logstate",
                c => c.Action<InterviewsApiV2Controller>(x => x.LogInterviewAsSuccessfullyHandled(Param.Any<Guid>())));
            config.TypedRoute("api/interviewer/v2/interviews/{id:guid}",
                c => c.Action<InterviewsApiV2Controller>(x => x.Post(Param.Any<InterviewPackageApiView>())));
            config.TypedRoute("api/interviewer/v2/interviews/{id:guid}/image",
                c => c.Action<InterviewsApiV2Controller>(x => x.PostImage(Param.Any<PostFileRequest>())));
            config.TypedRoute("api/interviewer/v2/interviews/{id:guid}/audio",
                c => c.Action<InterviewsApiV2Controller>(x => x.PostAudio(Param.Any<PostFileRequest>())));
            config.TypedRoute("api/interviewer/v2/attachments/{id}",
                c => c.Action<AttachmentsApiV2Controller>(x => x.GetAttachmentContent(Param.Any<string>())));
            config.TypedRoute("api/interviewer/v2/assignments", 
                c => c.Action<AssignmentsApiV2Controller>(x => x.GetAssignmentsAsync(Param.Any<CancellationToken>())));
            config.TypedRoute("api/interviewer/v2/assignments/{id}",
                c => c.Action<AssignmentsApiV2Controller>(x => x.GetAssignmentAsync(Param.Any<int>(), Param.Any<CancellationToken>())));
        }
        #pragma warning restore 4014
    }
}