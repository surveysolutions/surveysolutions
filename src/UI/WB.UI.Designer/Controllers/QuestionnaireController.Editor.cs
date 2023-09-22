using System;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.UI.Designer.Code.Vue;

namespace WB.UI.Designer.Controllers
{
    public partial class QuestionnaireController
    {
        [HttpGet]
        [VuePage]
        [Route("q/details")]
        public IActionResult QuestionnaireDetails(QuestionnaireRevision? id, Guid? chapterId, string entityType, Guid? entityId)
        {
            return Ok();
        }
    }
}
