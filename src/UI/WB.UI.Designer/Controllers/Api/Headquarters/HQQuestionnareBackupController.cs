using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Attributes;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    [Route("api/hq/backup")]
    [Authorize]
    public class HQQuestionnareBackupController : ControllerBase
    {
        private readonly IQuestionnaireHelper questionnaireHelper;
        public HQQuestionnareBackupController(IQuestionnaireHelper questionnaireHelper)
        {
            this.questionnaireHelper = questionnaireHelper;
        }

        [HttpGet]
        [Route("{questionnaireId}")]
        public IActionResult Get(Guid questionnaireId)
        {
            var stream = this.questionnaireHelper.GetBackupQuestionnaire(questionnaireId, out string questionnaireFileName);
            if (stream == null) return NotFound();

            return File(stream, "application/zip", $"{questionnaireFileName}.zip");
        }

    }
}
