using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    public class TranslationsApiV1Controller : TranslationsControllerBase
    {
        public TranslationsApiV1Controller(ITranslationManagementService translations) : base(translations)
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetTranslations)]
        [Route("api/supervisor/v1/translations/{id}")]
        public override ActionResult<List<TranslationDto>> Get(string id) => base.Get(id);
    }

}
