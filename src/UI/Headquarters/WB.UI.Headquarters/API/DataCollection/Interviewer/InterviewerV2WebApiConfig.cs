using System;
using System.ComponentModel;
using System.Threading;
using System.Web.Http;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v2;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer
{
    [Localizable(false)]
    public class InterviewerV2WebApiConfig
    {
#pragma warning disable 4014

        public static void Register(HttpConfiguration config)
        {
            config.TypedRoute(@"api/interviewer/v2/devices/info", c => c.Action<DevicesApiV2Controller>(x => x.Info(Param.Any<DeviceInfoApiView>())));
            config.TypedRoute(@"api/interviewer/v2/devices/statistics", c => c.Action<DevicesApiV2Controller>(x => x.Statistics(Param.Any<SyncStatisticsApiView>())));
            config.TypedRoute(@"api/interviewer/v2/devices/exception", c => c.Action<DevicesApiV2Controller>(x => x.UnexpectedException(Param.Any<UnexpectedExceptionApiView>())));

            config.TypedRoute("api/interviewer/v2", c => c.Action<InterviewerApiV2Controller>(x => x.Get()));
            config.TypedRoute("api/interviewer/v2/latestversion", c => c.Action<InterviewerApiV2Controller>(x => x.GetLatestVersion()));

            config.TypedRoute("api/interviewer/v2/tabletInfoAsFile",
                c => c.Action<InterviewerApiV2Controller>(x => x.PostTabletInformationAsFile()));
            config.TypedRoute("api/interviewer/v2/devices/current/{id}/{version}",
                c => c.Action<DevicesApiV2Controller>(x => x.CanSynchronize(Param.Any<string>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/devices/link/{id}/{version:int}",
                c => c.Action<DevicesApiV2Controller>(x => x.LinkCurrentResponsibleToDevice(Param.Any<string>(), Param.Any<int>())));
            config.TypedRoute("api/interviewer/v2/users/login", c => c.Action<UsersApiV2Controller>(x => x.Login(Param.Any<LogonInfo>())));
            config.TypedRoute("api/interviewer/v2/users/current", c => c.Action<UsersApiV2Controller>(x => x.Current()));
            config.TypedRoute("api/interviewer/v2/users/supervisor", c => c.Action<UsersApiV2Controller>(x => x.Supervisor()));
            config.TypedRoute("api/interviewer/v2/users/hasdevice", c => c.Action<UsersApiV2Controller>(x => x.HasDevice()));
            config.TypedRoute("api/interviewer/v2/translations/{id}",
                c => c.Action<TranslationsApiV2Controller>(x => x.Get(Param.Any<string>())));
            config.TypedRoute("api/interviewer/v2/companyLogo", c => c.Action<SettingsV2Controller>(x => x.CompanyLogo()));
            config.TypedRoute("api/interviewer/v2/autoupdate", c => c.Action<SettingsV2Controller>(x => x.AutoUpdateEnabled()));
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

            config.TypedRoute("api/interviewer/v2/attachments/{id}",
                c => c.Action<AttachmentsApiV2Controller>(x => x.GetAttachmentContent(Param.Any<string>())));
            config.TypedRoute("api/interviewer/v2/assignments",
                c => c.Action<AssignmentsApiV2Controller>(x => x.GetAssignmentsAsync(Param.Any<CancellationToken>())));
            config.TypedRoute("api/interviewer/v2/assignments/{id}",
                c => c.Action<AssignmentsApiV2Controller>(x => x.GetAssignmentAsync(Param.Any<int>(), Param.Any<CancellationToken>())));
            config.TypedRoute("api/interviewer/v2/maps", c => c.Action<MapsApiV2Controller>(x => x.GetMaps()));
            config.TypedRoute("api/interviewer/v2/maps/{id}",
                c => c.Action<MapsApiV2Controller>(x => x.GetMapContent((Param.Any<string>()))));
            config.TypedRoute("api/interviewer/v2/auditlog",
                c => c.Action<AuditLogApiV2Controller>(x => x.Post(Param.Any<AuditLogEntitiesApiView>())));

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

        }

#pragma warning restore 4014

    }
}
