using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [Authorize(Roles = "Interviewer")]
    public class TranslationsApiV2Controller : TranslationsControllerBase
    {
        public TranslationsApiV2Controller(ITranslationManagementService translations) : base(translations)
        {
        }

        [HttpGet]
        [Route("api/interviewer/v2/translations/{id}")]
        public override ActionResult<List<TranslationDto>> Get(string id) => base.Get(id);
    }
}
