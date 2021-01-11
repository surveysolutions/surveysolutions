using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class SettingsControllerBase : ControllerBase
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly IPlainStorageAccessor<ServerSettings> tenantSettings;
        private readonly ISecureStorage secureStorage;

        protected SettingsControllerBase(
            IPlainKeyValueStorage<CompanyLogo> appSettingsStorage,
            IPlainStorageAccessor<ServerSettings> tenantSettings,
            ISecureStorage secureStorage)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.tenantSettings = tenantSettings;
            this.secureStorage = secureStorage;
        }

        public virtual IActionResult CompanyLogo()
        {
            string etagHeaderValue = Request.Headers[HeaderNames.IfNoneMatch].FirstOrDefault();
            string incomingEtag = null;
            if (!string.IsNullOrEmpty(etagHeaderValue))
            {
                incomingEtag = EntityTagHeaderValue.Parse(etagHeaderValue).Tag.ToString();
            }

            var companyLogo = this.appSettingsStorage.GetById(AppSetting.CompanyLogoStorageKey);

            if (companyLogo == null) 
                return NoContent();

            var etagValue = companyLogo.GetEtagValue();
            if (etagValue.Equals(incomingEtag?.Replace("\"", ""), StringComparison.InvariantCultureIgnoreCase))
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            return File(companyLogo.Logo, "image/png", null, new EntityTagHeaderValue($"\"{etagValue}\""));
        }

        public virtual ActionResult<TenantIdApiView> TenantId()
        {
            var byId = tenantSettings.GetById(ServerSettings.PublicTenantIdKey);
            if(byId == null) 
                throw new NullReferenceException("Tenant public key is not set");
            return new TenantIdApiView
            {
                TenantId = byId.Value
            };
        }

        public virtual IActionResult AutoUpdateEnabled() =>  new JsonResult(false);

        public virtual IActionResult PublicKeyForEncryption() =>
            new JsonResult(Convert.ToBase64String(this.secureStorage.Retrieve(RsaEncryptionService.PublicKey)));

        public virtual IActionResult NotificationsEnabled() => new JsonResult(true);
    }
}
