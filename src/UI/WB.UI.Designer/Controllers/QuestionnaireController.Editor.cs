using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Code.Vue;

namespace WB.UI.Designer.Controllers
{
    public partial class QuestionnaireController
    {
        [HttpGet]
        [VuePage]
        [Route("q/details/{id}")]
        [Route("q/details/{id}/chapter/{chapterId}")]
        [Route("q/details/{id}/chapter/{chapterId}/{entityType}/{entityId}")]
        public IActionResult QuestionnaireDetails(QuestionnaireRevision? id, Guid? chapterId, string entityType, Guid? entityId)
        {
            return this.Ok();
        }
    }
}
