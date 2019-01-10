using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.UI.Headquarters.API.DataCollection
{
    public abstract class SettingsControllerBase : ApiController
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly ISecureStorage secureStorage;
        private readonly IPlainKeyValueStorage<InterviewerSettings> mobileAppsSettingsStorage;

        protected SettingsControllerBase(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage,
            ISecureStorage secureStorage,
            IPlainKeyValueStorage<InterviewerSettings> mobileAppsSettingsStorage)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.secureStorage = secureStorage;
            this.mobileAppsSettingsStorage = mobileAppsSettingsStorage;
        }

        public virtual HttpResponseMessage CompanyLogo()
        {
            var incomingEtag = Request.Headers.IfNoneMatch.FirstOrDefault()?.Tag ?? "";
            var companyLogo = this.appSettingsStorage.GetById(AppSetting.CompanyLogoStorageKey);

            if (companyLogo == null) return Request.CreateResponse(HttpStatusCode.NoContent);

            var etagValue = companyLogo.GetEtagValue();
            if (etagValue.Equals(incomingEtag.Replace("\"", ""), StringComparison.InvariantCultureIgnoreCase))
            {
                return Request.CreateResponse(HttpStatusCode.NotModified);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(companyLogo.Logo);
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(@"image/png");
            response.Headers.ETag = new EntityTagHeaderValue($"\"{etagValue}\"");

            return response;
        }

        public virtual bool AutoUpdateEnabled() =>
            this.mobileAppsSettingsStorage.GetById(AppSetting.InterviewerSettings).IsAutoUpdateEnabled();

        public virtual string PublicKeyForEncryption() =>
            Convert.ToBase64String(this.secureStorage.Retrieve(RsaEncryptionService.PublicKey));
    }
}
