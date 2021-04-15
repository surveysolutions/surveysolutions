using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2
{
    [Authorize(Roles = "Interviewer")]
    [Route("api/interviewer/v2")]
    public class SettingsV2Controller : SettingsControllerBase
    {
        private readonly IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires;
        private readonly IWebInterviewConfigProvider interviewConfigProvider;
        private readonly IWebInterviewLinkProvider webInterviewLinkProvider;
        
        public SettingsV2Controller(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage,
            IPlainStorageAccessor<ServerSettings> tenantSettings,
            ISecureStorage secureStorage, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires, 
            IWebInterviewLinkProvider webInterviewLinkProvider, 
            IWebInterviewConfigProvider interviewConfigProvider) 
            : base(appSettingsStorage, tenantSettings, secureStorage)
        {
            this.interviewerSettingsStorage = interviewerSettingsStorage;
            this.questionnaires = questionnaires;
            this.webInterviewLinkProvider = webInterviewLinkProvider;
            this.interviewConfigProvider = interviewConfigProvider;
        }

        [HttpGet]
        [Route("companyLogo")]
        public override IActionResult CompanyLogo() => base.CompanyLogo();

        [HttpGet]
        [Route("autoupdate")]
        public override IActionResult AutoUpdateEnabled() =>
            new JsonResult(
                this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings).IsAutoUpdateEnabled());

        [HttpGet]
        [Route("encryption-key")]
        public override IActionResult PublicKeyForEncryption() => base.PublicKeyForEncryption();

        [HttpGet]
        [Route("notifications")]
        public override IActionResult NotificationsEnabled() =>
            new JsonResult(this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings)
                .IsDeviceNotificationsEnabled());

        [HttpGet]
        [Route("tenantId")]
        public override ActionResult<TenantIdApiView> TenantId() => base.TenantId();

        [HttpGet]
        [Route("tabletsettings")]
        public RemoteTabletSettingsApiView TabletSettings() => new()
        {
            PartialSynchronizationEnabled = this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings)
                .IsPartialSynchronizationEnabled(),

            QuestionnairesPermittedToSwitchToWebMode = questionnaires
                .Query(_ => _.Where(q => !q.Disabled)
                    .Select(w => w.Id)
                    .ToList()
                ).Where(PermittedQuestionnaire)
                .ToList(),

            WebInterviewUrlTemplate = this.webInterviewLinkProvider.WebInterviewRequestLink(
                "{assignment}", "{interviewId}")
        };

        private bool PermittedQuestionnaire(string questionnaireId)
        {
            var properties= this.interviewConfigProvider.Get(QuestionnaireIdentity.Parse(questionnaireId));
            return properties.Started && properties.AllowSwitchToCawiForInterviewer;
        }
    }
}
