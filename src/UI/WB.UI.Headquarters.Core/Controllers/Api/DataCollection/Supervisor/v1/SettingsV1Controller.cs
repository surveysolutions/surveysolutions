using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Implementation;
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
            ISecureStorage secureStorage)
            : base(appSettingsStorage, settings, secureStorage)
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
    }
}
