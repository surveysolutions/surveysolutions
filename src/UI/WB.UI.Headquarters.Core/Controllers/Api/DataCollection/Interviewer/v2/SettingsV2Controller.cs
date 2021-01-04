using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2
{
    [Authorize(Roles = "Interviewer")]
    [Route("api/interviewer/v2")]
    public class SettingsV2Controller : SettingsControllerBase
    {
        private readonly IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage;

        public SettingsV2Controller(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage,
            IPlainStorageAccessor<ServerSettings> tenantSettings,
            ISecureStorage secureStorage) : base(appSettingsStorage, tenantSettings, secureStorage)
        {
            this.interviewerSettingsStorage = interviewerSettingsStorage;
        }

        [HttpGet]
        [Route("companyLogo")]
        public override IActionResult CompanyLogo() => base.CompanyLogo();

        [HttpGet]
        [Route("autoupdate")]
        public override IActionResult AutoUpdateEnabled() =>
            new JsonResult(this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings).IsAutoUpdateEnabled());

        [HttpGet]
        [Route("encryption-key")]
        public override IActionResult PublicKeyForEncryption() => base.PublicKeyForEncryption();

        [HttpGet]
        [Route("notifications")]
        public override IActionResult NotificationsEnabled() =>
            new JsonResult(this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings).IsDeviceNotificationsEnabled());

        [HttpGet]
        [Route("tenantId")]
        public override ActionResult<TenantIdApiView> TenantId() => base.TenantId();

        [HttpGet]
        [Route("tabletsettings")]
        public RemoteTabletSettingsApiView TabletSettings() =>
            new RemoteTabletSettingsApiView()
            {
                PartialSynchronizationEnabled = this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings).IsPartialSynchronizationEnabled()
            };
    }
}
