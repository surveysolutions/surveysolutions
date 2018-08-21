using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class SettingsV1Controller : SettingsControllerBase
    {
        public SettingsV1Controller(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage) : base(appSettingsStorage)
        {
        }

        [HttpGet]
        public override HttpResponseMessage CompanyLogo() => base.CompanyLogo();

        [HttpGet]
        public override bool AutoUpdateEnabled() => base.AutoUpdateEnabled();
    }
}
