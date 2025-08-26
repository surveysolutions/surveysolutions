using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Configs;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    [Route("api/supervisor/v1")]
    public class SettingsV1Controller : SettingsControllerBase
    {
        public SettingsV1Controller(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage, 
            IPlainStorageAccessor<ServerSettings> settings, 
            ISecureStorage secureStorage,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage,
            IWebInterviewLinkProvider webInterviewLinkProvider,
            IOptions<GoogleMapsConfig> googleMapsConfig)
            : base(appSettingsStorage, settings, secureStorage, interviewerSettingsStorage, webInterviewLinkProvider, googleMapsConfig)
        {
        }

        [HttpGet]
        [Route("companyLogo")]
        public override IActionResult CompanyLogo() => base.CompanyLogo();

        [HttpGet]
        [Route("autoupdate")]
        public override IActionResult AutoUpdateEnabled() => base.AutoUpdateEnabled();

        [HttpGet]
        [Route("encryption-key")]
        public override IActionResult PublicKeyForEncryption() => base.PublicKeyForEncryption();

        [HttpGet]
        [Route("notifications")]
        public override IActionResult NotificationsEnabled() => base.NotificationsEnabled();

        [HttpGet]
        [Route("tenantId")]
        public override ActionResult<TenantIdApiView> TenantId() => base.TenantId();
        
        [HttpGet]
        [Route("tabletsettings")]
        public override RemoteTabletSettingsApiView TabletSettings() => base.TabletSettings();
    }
}
