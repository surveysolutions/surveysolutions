using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Code;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    [Route("api/hq/backup")]
    [Authorize]
    public class HQQuestionnareBackupController : HQControllerBase
    {
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IQuestionnaireHelper questionnaireHelper;
        public HQQuestionnareBackupController(IQuestionnaireHelper questionnaireHelper, 
            IQuestionnaireViewFactory questionnaireViewFactory)
        {
            this.questionnaireHelper = questionnaireHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        [HttpGet]
        [Route("{questionnaireId}")]
        public IActionResult Get(Guid questionnaireId)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId));
            if (questionnaireView == null)
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status404NotFound, string.Format(ErrorMessages.TemplateNotFound, questionnaireId));
            }

            if (!this.ValidateAccessPermissions(questionnaireView))
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status403Forbidden, ErrorMessages.NoAccessToQuestionnaire);
            }

            var stream = this.questionnaireHelper.GetBackupQuestionnaire(questionnaireId, out string questionnaireFileName);
            if (stream == null) return NotFound();

            return File(stream, "application/zip", $"{questionnaireFileName}.zip");
        }
    }
}
