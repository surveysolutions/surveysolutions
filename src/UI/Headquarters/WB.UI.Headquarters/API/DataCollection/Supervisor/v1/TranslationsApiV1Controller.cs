using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Enumerator.Native.Questionnaire;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class TranslationsApiV1Controller : TranslationsControllerBase
    {
        public TranslationsApiV1Controller(ITranslationManagementService translations) : base(translations)
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetTranslations)]
        public override HttpResponseMessage Get(string id) => base.Get(id);
    }

}
