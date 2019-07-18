using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class SettingsV2Controller : SettingsControllerBase
    {
        private readonly IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage;

        public SettingsV2Controller(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage,
            IPlainKeyValueStorage<TenantSettings> tenantSettings,
            ISecureStorage secureStorage) : base(appSettingsStorage, tenantSettings, secureStorage)
        {
            this.interviewerSettingsStorage = interviewerSettingsStorage;
        }


        [HttpGet]
        public override HttpResponseMessage CompanyLogo() => base.CompanyLogo();

        [HttpGet]
        public override bool AutoUpdateEnabled() =>
            this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings).IsAutoUpdateEnabled();

        [HttpGet]
        public override string PublicKeyForEncryption() => base.PublicKeyForEncryption();

        [HttpGet]
        public override bool NotificationsEnabled() =>
            this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings).IsDeviceNotificationsEnabled();

        [HttpGet]
        public override HttpResponseMessage TenantId()
        {
            return base.TenantId();
        }
    }
}
