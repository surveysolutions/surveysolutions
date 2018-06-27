using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class TranslationsApiV2Controller : TranslationsControllerBase
    {
        public TranslationsApiV2Controller(ITranslationManagementService translations) : base(translations)
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetTranslations)]
        public override HttpResponseMessage Get(string id) => base.Get(id);

    }
}
